namespace SmartAlarmCFData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDeviceIDColunmtoRestorationAlarmListTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RestorationAlarmList", "DeviceID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RestorationAlarmList", "DeviceID");
        }
    }
}
