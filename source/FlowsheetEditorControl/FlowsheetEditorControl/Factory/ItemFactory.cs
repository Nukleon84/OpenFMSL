using FlowsheetEditorControl.Items;
using OpenFMSL.Core.Flowsheeting;
using System.Diagnostics;


namespace FlowsheetEditor.Factory
{
    public static class ItemFactory
    {
        public static VisualUnit Create(IconTypes name, double x, double y)
        {
            switch (name)
            {
                case IconTypes.Variable:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Variable01";
                        newItem.Type = "Variable";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "Transparent";
                        newItem.IsLabelVisible = false;
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 20;
                        newItem.Width = 200;
                        newItem.DisplayIcon = IconTypes.Variable;                        
                        return newItem;
                    }
                case IconTypes.PIAdapter:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "PIAdapter01";
                        newItem.Type = "PIAdapter";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "GhostWhite";
                        newItem.IsLabelVisible = false;
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 300;
                        newItem.Width = 200;
                        newItem.DisplayIcon = IconTypes.PIAdapter;
                       
                        return newItem;
                    }
                case IconTypes.Spreadsheet:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Spreadsheet01";
                        newItem.Type = "Spreadsheet";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "GhostWhite";
                        newItem.IsLabelVisible = false;
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 300;
                        newItem.Width = 200;
                        newItem.DisplayIcon = IconTypes.Spreadsheet;
                      
                        return newItem;
                    }
                case IconTypes.Button:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Button01";
                        newItem.Type = "Button";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "GhostWhite";
                        newItem.IsLabelVisible = false;
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 100;
                        newItem.DisplayIcon = IconTypes.Button;
                       
                        return newItem;
                    }
                case IconTypes.Equation:                
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Equation01";
                        newItem.Type = "Equation";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 80;
                        newItem.DisplayIcon = IconTypes.Equation;
                        newItem.Report = "Click to enter equation...";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "DimGray";
                        return newItem;
                    }
                case IconTypes.Text:
                case IconTypes.None:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "Label01";
                        newItem.Type = "Text";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 50;
                        newItem.Width = 50;
                        newItem.DisplayIcon = IconTypes.Text;
                        newItem.Report = "Enter your text here...";
                        newItem.FillColor = "GhostWhite";
                        newItem.BorderColor = "GhostWhite";
                        return newItem;
                    }
                case IconTypes.Feed:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "S01";
                        newItem.Type = "MaterialStream";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 25;
                        newItem.Width = 50;
                        newItem.DisplayIcon = IconTypes.Stream;                     
                        var inlet = new Connector
                        {
                            Name = "Stream",
                            Type = "Material",
                            X = 45,
                            Y = 15,
                            Owner = newItem,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Right
                        };
                        newItem.Connectors.Add(inlet);
                        return newItem;
                    }
                case IconTypes.Product:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "P01";
                        newItem.Type = "MaterialStream";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 25;
                        newItem.Width = 50;
                        newItem.DisplayIcon = IconTypes.Stream;

                        var inlet = new Connector
                        {
                            Name = "Stream",
                            Type = "Material",
                            X = -5,
                            Y = 15,
                            Owner = newItem,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(inlet);
                        return newItem;
                    }
                case IconTypes.Breaker:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "BR01";
                        newItem.Type = "Breaker";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.Breaker;
                 
                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = newItem.Width - 5,

                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Right,
                            Intent = ConnectorIntent.Inlet
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Out",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            Direction=ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(outlet1);
                        return newItem;
                    }

                case IconTypes.TwoPhaseFlash:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "FLA01";
                        newItem.Type = "Flash2";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 40;
                        newItem.DisplayIcon = IconTypes.TwoPhaseFlash;
                            var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = 15,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Vap",
                            Type = "Material",
                            X = newItem.Width/2.0 - 5,
                            Y = -5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Up
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Liq",
                            Type = "Material",
                            X = newItem.Width/2.0 - 5,
                            Y = newItem.Height - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Down
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }
                case IconTypes.FallingFilm:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "FAFI01";
                        newItem.Type = "Flash2";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 100;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.FallingFilm;
                                             
                           var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = newItem.Width / 2 - 5,
                            Y = -5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Up
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Vap",
                            Type = "Material",
                            X = -5,
                            Y = newItem.Height - 20,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Liq",
                            Type = "Material",
                            X = newItem.Width / 2 - 5,
                            Y = newItem.Height - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Down
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }
                case IconTypes.ThreePhaseFlash:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "F301";
                        newItem.Type = "Flash3";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 100;
                        newItem.Width = 75;
                        newItem.DisplayIcon = IconTypes.ThreePhaseFlash;               
                        //newItem.SimulationObject = ModelFactory.Create(system, newItem);
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = -5,
                            Y = 45,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Outlet[1]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = 35,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet2);

                        var outlet3 = new Connector
                        {
                            Name = "Outlet[2]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = newItem.Height - 25,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet3);
                        return newItem;
                    }
                case IconTypes.Decanter:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "DEC01";
                        newItem.Type = "Decanter";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 80;
                        newItem.Width = 120;
                        newItem.DisplayIcon = IconTypes.Decanter;
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.NozzleLeft
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.NozzleRight
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Outlet[1]",
                            Type = "Material",
                            X = newItem.Width / 2.0 - 5,
                            Y = newItem.Height - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.NozzleBottom
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }
                case IconTypes.Splitter:
                    {
                        var newItem = new VisualUnit
                        {
                            Name = "SPLIT01",
                            Type = "Splitter",
                            X = x,
                            Y = y,
                            Height = 40,
                            Width = 40,
                            DisplayIcon = IconTypes.Splitter,                

                        };

                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Out1",
                            Type = "Material",
                            X = newItem.Width/2.0 - 5,
                            Y = -5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Up
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Out2",
                            Type = "Material",
                            X = newItem.Width/2.0 - 5,
                            Y = newItem.Height - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Down
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }

                case IconTypes.Mixer:
                    {
                        var newItem = new VisualUnit
                        {
                            Name = "MIX01",
                            Type = "Mixer",
                            X = x,
                            Y = y,
                            Height = 40,
                            Width = 40,
                            DisplayIcon = IconTypes.Mixer,
                           };
                  
                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(inlet);
                                        
                        var outlet1 = new Connector
                        {
                            Name = "Out",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            IconType = ConnectorIconTypes.Box,
                            Intent = ConnectorIntent.Outlet,
                            Direction = ConnectorDirection.Right
                        };
                        newItem.Connectors.Add(outlet1);

                        return newItem;
                    }
                case IconTypes.ComponentSplitter:
                    {
                        var newItem = new VisualUnit
                        {
                            Name = "CSPLIT01",
                            Type = "ComponentSplitter",
                            X = x,
                            Y = y,
                            Height = 60,
                            Width = 60,
                            DisplayIcon = IconTypes.Block                            
                        };
                        //  newItem.SimulationObject = ModelFactory.Create(system, newItem);
                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Out1",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Out2",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = newItem.Height - 15,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }

                case IconTypes.Column:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "COL01";
                        newItem.Type = "Column";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 260;
                        newItem.Width = 100;
                        newItem.DisplayIcon = IconTypes.Column;
                     
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = -5,
                            Y = newItem.Height / 2.0 - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet3 = new Connector
                        {
                            Name = "Vapor",
                            Type = "Material",
                            X = newItem.Width - 40,
                            Y = -5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet3);


                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 40,
                            Y = 35,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);

                        var outlet2 = new Connector
                        {
                            Name = "Outlet[1]",
                            Type = "Material",
                            X = newItem.Width - 40,
                            Y = newItem.Height - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet2);
                        return newItem;
                    }

                case IconTypes.ColumnSection:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "SEC01";
                        newItem.Type = "ColumnSection";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 300;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.ColumnSection;
                      
                        var inlet = new Connector
                        {
                            Name = "Feeds",
                            Type = "Material",
                            X = -5,
                            Y = newItem.Height / 2.0 - 5,
                            Owner = newItem,
                            Intent = ConnectorIntent.Inlet,
                            Direction = ConnectorDirection.Left
                        };
                        newItem.Connectors.Add(inlet);

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LOut",
                            Type = "Material",
                            X = newItem.Width / 2.0 - 5,
                            Y = newItem.Height - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Down,
                            Intent = ConnectorIntent.Outlet
                        });
                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VOut",
                            Type = "Material",
                            X = newItem.Width / 2.0 - 5,
                            Y = -5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Up,
                            Intent = ConnectorIntent.Outlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LIn",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = 30,
                            Owner = newItem,
                            Direction = ConnectorDirection.Right,
                            Intent = ConnectorIntent.Inlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VIn",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = newItem.Height - 30,
                            Owner = newItem,
                            Direction = ConnectorDirection.Right,
                            Intent = ConnectorIntent.Inlet

                        });
                        newItem.Connectors.Add(new Connector
                        {
                            Name = "Sidestreams",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = newItem.Height / 2.0 - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Right,
                            Intent = ConnectorIntent.Outlet
                        });

                        return newItem;
                    }
                case IconTypes.RateBasedSection:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "SEC01";
                        newItem.Type = "RateBasedSection";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 160;
                        newItem.Width = 80;
                        newItem.DisplayIcon = IconTypes.RateBasedSection;

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LOut",
                            Type = "Material",
                            X = newItem.Width - 15,
                            Y = newItem.Height - 10,
                            Owner = newItem,
                            Direction = ConnectorDirection.Down,
                            Intent = ConnectorIntent.Outlet
                        });
                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VOut",
                            Type = "Material",
                            X =  5,
                            Y = 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Up,
                            Intent = ConnectorIntent.Outlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LIn",
                            Type = "Material",
                            X = newItem.Width - 15,
                            Y = 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Up,
                            Intent = ConnectorIntent.Inlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VIn",
                            Type = "Material",
                            X = 5,
                            Y = newItem.Height - 10,
                            Owner = newItem,
                            Direction = ConnectorDirection.Down,
                            Intent = ConnectorIntent.Inlet

                        });                       

                        return newItem;
                    }

                case IconTypes.FeedStage:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "EQ01";
                        newItem.Type = "FeedStage";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 60;
                        newItem.Width = 80;
                        newItem.DisplayIcon = IconTypes.FeedStage;

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = newItem.Height/2 - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Left,
                            Intent = ConnectorIntent.Inlet
                        });


                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LOut",
                            Type = "Material",
                            X = newItem.Width - 15,
                            Y = newItem.Height - 10,
                            Owner = newItem,
                            Direction = ConnectorDirection.Down,
                            Intent = ConnectorIntent.Outlet
                        });
                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VOut",
                            Type = "Material",
                            X = 5,
                            Y = 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Up,
                            Intent = ConnectorIntent.Outlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "LIn",
                            Type = "Material",
                            X = newItem.Width - 15,
                            Y = 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Up,
                            Intent = ConnectorIntent.Inlet
                        });

                        newItem.Connectors.Add(new Connector
                        {
                            Name = "VIn",
                            Type = "Material",
                            X = 5,
                            Y = newItem.Height - 10,
                            Owner = newItem,
                            Direction = ConnectorDirection.Down,
                            Intent = ConnectorIntent.Inlet

                        });

                        return newItem;
                    }


                case IconTypes.UserModel:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "U01";
                        newItem.Type = "UserModel";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 60;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.Block;                  
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);

                        return newItem;
                    }
                case IconTypes.Heater:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "H01";
                        newItem.Type = "Heater";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 40;
                        newItem.DisplayIcon = IconTypes.Heater;                  
                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Left,
                            Intent = ConnectorIntent.Inlet
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Out",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Right,
                            Intent = ConnectorIntent.Outlet
                        };
                        newItem.Connectors.Add(outlet1);


                        return newItem;
                    }

                case IconTypes.Pump:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "P01";
                        newItem.Type = "Pump";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 40;
                        newItem.DisplayIcon = IconTypes.Pump;                  
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = (int)(newItem.Width / 2.0) - 5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Left,
                            Intent = ConnectorIntent.Inlet
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 10,
                            Y = -5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);


                        return newItem;
                    }
                case IconTypes.Valve:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "VA01";
                        newItem.Type = "Valve";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 40;
                        newItem.DisplayIcon = IconTypes.Valve;                 
                        var inlet = new Connector
                        {
                            Name = "In",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Left,
                            Intent = ConnectorIntent.Inlet
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Out",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2.0) - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);
                        return newItem;
                    }

                case IconTypes.Compressor:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "C01";
                        newItem.Type = "Compressor";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 40;
                        newItem.DisplayIcon = IconTypes.Compressor;
                
                        var inlet = new Connector
                        {
                            Name = "Inlet[0]",
                            Type = "Material",
                            X = -5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem,
                            Direction = ConnectorDirection.Left,
                            Intent = ConnectorIntent.Inlet
                        };
                        newItem.Connectors.Add(inlet);

                        var outlet1 = new Connector
                        {
                            Name = "Outlet[0]",
                            Type = "Material",
                            X = newItem.Width - 5,
                            Y = (int)(newItem.Height / 2) - 5,
                            Owner = newItem
                        };
                        newItem.Connectors.Add(outlet1);


                        return newItem;
                    }
               
                case IconTypes.HeatExchanger:
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "HEX01";
                        newItem.Type = "HeatExchanger";
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 40;
                        newItem.Width = 140;
                        newItem.DisplayIcon = IconTypes.HeatExchanger;
                     
                        var inlet = new Connector();
                        inlet.Name = "TubeIn";
                        inlet.Type = "Material";
                        inlet.Direction = ConnectorDirection.Left;
                       // inlet.IconType = ConnectorIconTypes.NozzleLeft;
                        inlet.X = -5;
                        inlet.Y = (int)(newItem.Height / 2) - 5;
                        inlet.Owner = newItem;
                        newItem.AddConnector(inlet);

                        var outlet1 = new Connector();
                        outlet1.Name = "TubeOut";
                        outlet1.Type = "Material";
                        outlet1.Direction = ConnectorDirection.Right;
                        //outlet1.IconType = ConnectorIconTypes.NozzleRight;
                        outlet1.X = newItem.Width - 5;
                        outlet1.Y = (int)(newItem.Height / 2) - 5;
                        newItem.AddConnector(outlet1);



                        var inlet2 = new Connector();
                        inlet2.Name = "ShellIn";
                        inlet2.Type = "Material";
                        inlet2.Direction = ConnectorDirection.Up;
                        inlet2.X = newItem.Width - 35;
                        inlet2.Y = -5;
                        newItem.AddConnector(inlet2);

                        var outlet2 = new Connector();
                        outlet2.Name = "ShellOut";
                        outlet2.Type = "Material";
                        outlet2.Direction = ConnectorDirection.Down;

                        outlet2.X = 25;
                        outlet2.Y = newItem.Height - 5;
                        newItem.AddConnector(outlet2);


                        return newItem;
                    }

                default:                
                    {
                        var newItem = new VisualUnit();
                        newItem.Name = "U01";                        
                        newItem.X = x;
                        newItem.Y = y;
                        newItem.Height = 60;
                        newItem.Width = 60;
                        newItem.DisplayIcon = IconTypes.Block;     
                        return newItem;
                    }

            }
            
        }
    }
}
