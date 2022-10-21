using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace BoardSquares.Models
{
    public class BoardSquaresContext: DbContext
    {
        private string schemaName = "dbo";
        public BoardSquaresContext(): base("BoardSquaresDB")
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BoardSquaresContext>(null);
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
            modelBuilder.HasDefaultSchema(schemaName);
            modelBuilder.Entity<Player>().ToTable("FP_Players", schemaName);
            modelBuilder.Entity<Team>().ToTable("FP_Teams", schemaName);
            modelBuilder.Entity<User>().ToTable("FP_Users", schemaName);
            modelBuilder.Entity<UserTeam>().ToTable("FP_UserTeams", schemaName);
            modelBuilder.Entity<UserTeamPlayer>().ToTable("FP_UserTeamPlayers", schemaName);
            modelBuilder.Entity<Game>().ToTable("FP_GameNumbers", schemaName);
            modelBuilder.Entity<Scoring>().ToTable("FP_ScoringWorkSheet", schemaName);
            modelBuilder.Entity<ScoringPoints>().ToTable("FP_Scoring", schemaName);
            modelBuilder.Entity<UserTeamTieBreakerPlayers>().ToTable("FP_UserTeamTieBreakerPlayers", schemaName);
            modelBuilder.Entity<News>().ToTable("FP_News", schemaName);
        }

        //public DbSet<Board> Boards { get; set; }

        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }
        public DbSet<UserTeamPlayer> UserTeamPlayers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Scoring> Scorings { get; set; }
        public DbSet<ScoringPoints> ScoringPoints { get; set; }
        public DbSet<UserTeamTieBreakerPlayers> TieBreakerPlayers { get; set; }
        public DbSet<News> News { get; set; }
    }
}