/*
********************************************************************************
MinerOS
A script to display some useful stats when using mining vehicles, oriented to
wheel-based surface vehicles, but can be adapted to other types.
********************************************************************************
*/

/* TODO
 * - check cargo and power capacity variables of the local grid, not the entire
 * lot when docked
 * - tidy up the code e.g. move vars to init i.e. Program() etc.
 */

public Program()
{
    // The constructor, called only once every session and
    // always before any other method is called. Use it to
    // initialize your script. 
    //     
    // The constructor is optional and can be removed if not
    // needed.
    // 
    // It's recommended to set RuntimeInfo.UpdateFrequency 
    // here, which will allow your script to run itself without a 
    // timer block.

    Runtime.UpdateFrequency = UpdateFrequency.Update100;;
}

public void Save()
{
    // Called when the program needs to save its state. Use
    // this method to save your state to the Storage field
    // or some other means. 
    // 
    // This method is optional and can be removed if not
    // needed.
}

public void Main(string argument, UpdateType updateSource)
{
    // The main entry point of the script, invoked every time
    // one of the programmable block's Run actions are invoked,
    // or the script updates itself. The updateSource argument
    // describes where the update came from.
    // 
    // The method itself is required, but the arguments above
    // can be removed if not needed.

    int versionMajor = 1;
    int versionMinor = 0;

    float radToDeg = 57.2958F; // how many degrees is one radian

    // grab cockpit LCD panel
    IMyCockpit cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
    if (cockpit == null) {
        Echo("Could not find cockpit block");
        return;
    }

    string ccpSurfCountTxt = String.Format("cockpit text surface count: {0:D}", cockpit.SurfaceCount);
    Echo(ccpSurfCountTxt);

    IMyTextSurface ccpLcd1 = cockpit.GetSurface(1) as IMyTextSurface;
    if (ccpLcd1 == null) {
        Echo("Could not find cockpit text surface (LCD)");
        return;
    }

    // get drill vertical hinge angles
    string blockName = "Vert Hinge L";
    IMyMotorStator drillHingeLV = GridTerminalSystem.GetBlockWithName(blockName) as IMyMotorStator;
    if (drillHingeLV == null) {
        Echo ("Could not find block: " + blockName);
    }

    blockName = "Vert Hinge R";
    IMyMotorStator drillHingeRV = GridTerminalSystem.GetBlockWithName(blockName) as IMyMotorStator;
    if (drillHingeRV == null) {
        Echo ("Could not find block: " + blockName);
    }

    float hingeLVDeg = drillHingeLV.Angle * radToDeg;
    float hingeRVDeg = drillHingeRV.Angle * radToDeg;

    // get battery charge
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(batteries);

    float maxChargeTotalKwh = 0.0F;
    foreach (var battery in batteries) {
        maxChargeTotalKwh += battery.MaxStoredPower * 1000;
    }
    float chargeTotalKwh = 0.0F;
    foreach (var battery in batteries) {
        chargeTotalKwh += battery.CurrentStoredPower * 1000;
    }
    float chargePercent = (chargeTotalKwh / maxChargeTotalKwh) * 100;

    // get cargo space
    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType(cargoContainers);

    float maxCargoTotal = 0.0F;
    foreach (var cargoContainer in cargoContainers) {
        maxCargoTotal += cargoContainer.GetInventory().MaxVolume.ToIntSafe();
    }
    float cargoTotal = 0.0F;
    foreach (var cargoContainer in cargoContainers) {
        cargoTotal += cargoContainer.GetInventory().CurrentVolume.ToIntSafe();
    }
    float cargoPercent = (cargoTotal / maxCargoTotal) * 100;

    // display on LCD
    ccpLcd1.WriteText($"*** MinerOS v{versionMajor}.{versionMinor} ***\n");
    ccpLcd1.WriteText("- Drill vert. ang. -\n", true);
    ccpLcd1.WriteText($"L:{hingeLVDeg:F1} R:{hingeRVDeg:F1}\n", true);
    ccpLcd1.WriteText("------ Cargo -------\n", true);
    ccpLcd1.WriteText($"{cargoPercent:F1}%: {cargoTotal:F0}/{maxCargoTotal:F0} m3\n", true);
    ccpLcd1.WriteText("------ Power -------\n", true);
    ccpLcd1.WriteText($"{chargePercent:F1}%: {chargeTotalKwh:F0}/{maxChargeTotalKwh:F0} kWh\n", true);
}
