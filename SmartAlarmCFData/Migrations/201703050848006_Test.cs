namespace SmartAlarmCFData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DigitalPointInfo", "MACName", c => c.String(maxLength: 255, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DigitalPointInfo", "MACName", c => c.String(maxLength: 100, unicode: false));
        }
    }
}
