namespace TangoApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cities",
                c => new
                    {
                        CityId = c.Int(nullable: false, identity: true),
                        CityName = c.String(),
                        CountryId = c.Int(nullable: false),
                        Country_CoutryId = c.Int(),
                    })
                .PrimaryKey(t => t.CityId)
                .ForeignKey("dbo.Countries", t => t.Country_CoutryId)
                .Index(t => t.Country_CoutryId);
            
            CreateTable(
                "dbo.Countries",
                c => new
                    {
                        CoutryId = c.Int(nullable: false, identity: true),
                        CountryName = c.String(),
                    })
                .PrimaryKey(t => t.CoutryId);
            
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        ProfileId = c.Int(nullable: false, identity: true),
                        ProfileVisibility = c.Boolean(nullable: false),
                        Description = c.String(),
                        Gender = c.Boolean(nullable: false),
                        Birthday = c.DateTime(nullable: false),
                        CityId = c.Int(nullable: false),
                        CountryId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Country_CoutryId = c.Int(),
                    })
                .PrimaryKey(t => t.ProfileId)
                .ForeignKey("dbo.Cities", t => t.CityId, cascadeDelete: true)
                .ForeignKey("dbo.Countries", t => t.Country_CoutryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.CityId)
                .Index(t => t.UserId)
                .Index(t => t.Country_CoutryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Profiles", "Country_CoutryId", "dbo.Countries");
            DropForeignKey("dbo.Profiles", "CityId", "dbo.Cities");
            DropForeignKey("dbo.Cities", "Country_CoutryId", "dbo.Countries");
            DropIndex("dbo.Profiles", new[] { "Country_CoutryId" });
            DropIndex("dbo.Profiles", new[] { "UserId" });
            DropIndex("dbo.Profiles", new[] { "CityId" });
            DropIndex("dbo.Cities", new[] { "Country_CoutryId" });
            DropTable("dbo.Profiles");
            DropTable("dbo.Countries");
            DropTable("dbo.Cities");
        }
    }
}
