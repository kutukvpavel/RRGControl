using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Collections.Generic;
using RRGControl.Models;
using System;
using System.Reactive.Linq;
using System.Linq; 
using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace RRGControl.ViewModels
{


    public class CreateScriptViewModel : ReactiveObject
    {
        private string _scriptName = "New Script";
        public string ScriptName { get => _scriptName; set => this.RaiseAndSetIfChanged(ref _scriptName, value); }
        private string _comment = "";
        public string Comment { get => _comment; set => this.RaiseAndSetIfChanged(ref _comment, value); }
        public ObservableCollection<UnitSetpointModel> InputUnits { get; } = new ObservableCollection<UnitSetpointModel>();
        private double _newDuration = 10; //default 10 sec
        public double NewDuration { get => _newDuration; set => this.RaiseAndSetIfChanged(ref _newDuration, value); }
        public ObservableCollection<ScriptCommandModel> Commands { get; } = new ObservableCollection<ScriptCommandModel>();
        private ScriptCommandModel? _selectedCommand;
        public ScriptCommandModel? SelectedCommand { get => _selectedCommand; set => this.RaiseAndSetIfChanged(ref _selectedCommand, value); }
        private int _requestPlotUpdate;
        public int RequestPlotUpdate 
        { 
            get => _requestPlotUpdate; 
            set => this.RaiseAndSetIfChanged(ref _requestPlotUpdate, value); 
        }

        private Dictionary<string, (double[] Xs, double[] Ys)> _plotData = new();
        public Dictionary<string, (double[] Xs, double[] Ys)> PlotData 
        { 
            get => _plotData; 
            set => this.RaiseAndSetIfChanged(ref _plotData, value); 
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveScriptCommand { get; }

        public CreateScriptViewModel(Dictionary<string, string> unitsMap, ObservableCollection<string> gases)
        {
            foreach (var kvpair in unitsMap)
            {
                InputUnits.Add(new UnitSetpointModel 
                { 
                    UnitId = kvpair.Key, 
                    UnitName = kvpair.Value, 
                    Setpoint = 0 
                });
            }

            AddCommand = ReactiveCommand.Create(AddCommandExecute);
            RemoveCommand = ReactiveCommand.Create(RemoveCommandExecute);
            SaveScriptCommand = ReactiveCommand.Create(SaveScriptExecute);

            Commands.CollectionChanged += (s, e) => RecalculatePlotData();
        }

        private void AddCommandExecute()
        {
            if (NewDuration <= 0) return;

            ScriptCommandModel newStep = new ScriptCommandModel();
            newStep.Duration = NewDuration;

            foreach (var input in InputUnits)
            {
                UnitSetpointModel unitCopy = new UnitSetpointModel();
                unitCopy.UnitId = input.UnitId;
                unitCopy.UnitName = input.UnitName;
                unitCopy.Setpoint = input.Setpoint;

                newStep.UnitSetpoints.Add(unitCopy);
            }
            Commands.Add(newStep);
        }

        private void RemoveCommandExecute()
        {
            Commands.Remove(SelectedCommand!); 
            SelectedCommand = null;
        }

        private void SaveScriptExecute()
        {
            try
            {
                if (Commands.Count == 0) return;
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptsDir = Path.Combine(appFolder, "scripts");
                string filePath = Path.Combine(scriptsDir, ScriptName + ".json");

                var commandList = new List<object>();
                
                var unitIsRegulating = new Dictionary<string, bool>();
                var allPossibleUnits = InputUnits.Select(u => u.UnitName).ToList();
                foreach (var name in allPossibleUnits) unitIsRegulating[name] = false;

                foreach (var step in Commands)
                {
                    var setpointsInStep = step.UnitSetpoints.ToList();
                    int technicalSeconds = 0;

                    // closing
                    foreach (var sp in setpointsInStep.Where(s => s.Setpoint <= 0))
                    {
                        if (unitIsRegulating[sp.UnitName])
                        {
                            commandList.Add(new { Duration = 1, Command = new { UnitName = sp.UnitName, RegisterName = "Setpoint", Value = "0", ConvertUnits = true } });
                            commandList.Add(new { Duration = 1, Command = new { UnitName = sp.UnitName, RegisterName = "OperationMode", Value = "Closed" } });
                            
                            unitIsRegulating[sp.UnitName] = false;
                            technicalSeconds += 2;
                        }
                    }

                    // opening
                    foreach (var sp in setpointsInStep.Where(s => s.Setpoint > 0))
                    {
                        if (!unitIsRegulating[sp.UnitName])
                        {
                            commandList.Add(new { Duration = 1, Command = new { UnitName = sp.UnitName, RegisterName = "OperationMode", Value = "Regulate" } });
                            unitIsRegulating[sp.UnitName] = true;
                            technicalSeconds += 1;
                        }
                    }

                    // setpoints
                    var activeFlows = setpointsInStep.Where(s => s.Setpoint > 0).ToList();
                    
                    for (int i = 0; i < activeFlows.Count; i++)
                    {
                        var u = activeFlows[i];
                        bool isLast = (i == activeFlows.Count - 1);

                        double duration = isLast 
                            ? Math.Max(1, step.Duration - technicalSeconds - (activeFlows.Count - 1)) 
                            : 1;

                        commandList.Add(new { 
                            Duration = duration, 
                            Command = new { 
                                UnitName = u.UnitName, 
                                RegisterName = "Setpoint", 
                                Value = u.Setpoint.ToString("G", System.Globalization.CultureInfo.InvariantCulture), 
                                ConvertUnits = true 
                            } 
                        });
                    }

                    // handle pause steps (all units closed)
                    if (activeFlows.Count == 0)
                    {
                        commandList.Add(new { Duration = Math.Max(1, step.Duration - technicalSeconds), Command = new { UnitName = allPossibleUnits.First(), 
                        RegisterName = "Setpoint", Value = "0" } });
                    }
                }

                foreach (var kvpair in unitIsRegulating)
                {
                    if (kvpair.Value == true)
                    {
                        string unitName = kvpair.Key;

                        commandList.Add(new { Duration = 1, Command = new { UnitName = unitName, RegisterName = "Setpoint", Value = "0", ConvertUnits = true } });
                        commandList.Add(new { Duration = 1, Command = new { UnitName = unitName, RegisterName = "OperationMode", Value = "Closed" } });
                    }
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(new { Name = ScriptName, Comment = Comment, Commands = commandList }, options));
            }
            catch (Exception ex) 
                { 
                Debug.WriteLine(ex.Message); 
                }
        }

        public void RecalculatePlotData()
        {
            if (Commands.Count == 0 || InputUnits.Count == 0)
            {
                PlotData = new Dictionary<string, (double[] Xs, double[] Ys)>();
                RequestPlotUpdate++;
                return;
            }

            var unitFlows = new Dictionary<string, List<(double Time, double Flow)>>();
            var allUnitNames = InputUnits.Select(u => u.UnitName).ToList();

            foreach (var name in allUnitNames)
            {
                unitFlows[name] = new List<(double, double)> { (0, 0) };
            }

            double currentTime = 0;
            
            var currentLevels = allUnitNames.ToDictionary(n => n, n => 0.0);

            foreach (var step in Commands)
            {
                double endTime = currentTime + step.Duration;

                foreach (var name in allUnitNames)
                {
                    double setpoint = 0;
                    foreach (var s in step.UnitSetpoints)
                    {
                        if (s.UnitName == name)
                        {
                            setpoint = s.Setpoint;
                            break;
                        }
                    }
                    var previousLevel = currentLevels[name];

                    if (currentTime > 0)
                    {
                        unitFlows[name].Add((currentTime, previousLevel));
                    }

                    unitFlows[name].Add((currentTime, setpoint));

                    unitFlows[name].Add((endTime, setpoint));

                    currentLevels[name] = setpoint;
                }
                currentTime = endTime;
            }

            var newData = new Dictionary<string, (double[] Xs, double[] Ys)>();

                foreach (var kvpair in unitFlows)
                {
                    var pointsList = kvpair.Value;
                    int count = pointsList.Count;

                    double[] xArray = new double[count];
                    double[] yArray = new double[count];

                    for (int j = 0; j < count; j++)
                    {
                        xArray[j] = pointsList[j].Time;
                        yArray[j] = pointsList[j].Flow;
                    }

                    newData[kvpair.Key] = (xArray, yArray);
                }

            PlotData = newData;
            RequestPlotUpdate++;
        }
    }
}