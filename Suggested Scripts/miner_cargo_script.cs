//========= FOR MINER ============ 
//Miner cargo script
// by KanKub 
 
string LCD_NAME = "LCD"; 
int LCD_MAX = 23; 
 
public void Main(string argument) 
{ 
 
    if(argument.ToLower() == "cargo") 
    { 
        VRage.MyFixedPoint inv_max = 0; 
        VRage.MyFixedPoint inv_cur = 0; 
 
        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); 
        GridTerminalSystem.GetBlocks(blocks); 
 
        foreach(IMyTerminalBlock b in blocks) 
        { 
                    if (b.HasInventory()&& !(b is IMyReactor)) 
                    { 
                        IMyInventory inv = b.GetInventory(b.GetInventoryCount() - 1); 
                        inv_max += inv.MaxVolume; 
                        inv_cur += inv.CurrentVolume; 
                    } 
 
        } 
 
        IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(LCD_NAME) as IMyTextPanel; 
 
        string s = new String('|',(int)((((double)inv_cur/(double)inv_max)*LCD_MAX))); 
        Echo("Current Inventory Volume\n"+((double)inv_cur*1000).ToString("N2") +" / "+((double)inv_max*1000).ToString("N2") + " L"); 
        lcd.WritePublicText(s,false); 
 
    } 
 
}