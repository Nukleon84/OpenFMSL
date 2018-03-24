namespace FlowsheetEditorControl.Items
{
    public enum ConnectorIconTypes
    {
        Box,
        NozzleLeft,
        NozzleRight,
        NozzleTop,
        NozzleBottom,
    }
    
      public enum ConnectorDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum ConnectorIntent
    {
        Inlet,
        Outlet,
        Unspecified
    }

    public enum ConnectionTypes
    {
        Material,
        Energy,
        Power
    }
}