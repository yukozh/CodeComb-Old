using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using CodeComb.Entity;

namespace CodeComb.Database
{
    public class DB : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Clarification> Clarifications { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<ContestManager> ContestManagers { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Glance> Glances { get; set; }
        public DbSet<Lock> Locks { get; set; }
        public DbSet<Hack> Hacks { get; set; }
        public DbSet<JudgeNode> JudgeNodes { get; set; }
        public DbSet<JudgeTask> JudgeTasks { get; set; }
        public DbSet<PrintRequest> PrintRequests { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TestCase> TestCases { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<AlgorithmTag> AlgorithmTags { get; set; }
        public DbSet<SolutionTag> SolutionTags { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Solution> Solutions { get; set; }

        public DB()
            : base("mysqldb")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Ratings)
                .WithRequired(r => r.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Tokens)
                .WithRequired(t => t.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Apps)
                .WithRequired(a => a.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Solutions)
                .WithRequired(s => s.User);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PMSent)
                .WithRequired(p => p.Sender);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PMReceived)
                .WithRequired(p => p.Receiver);

            modelBuilder.Entity<Contest>()
               .HasMany(c => c.Clarifications)
               .WithRequired(c => c.Contest);

            modelBuilder.Entity<Contest>()
               .HasMany(c => c.Problems)
               .WithRequired(p => p.Contest);

            modelBuilder.Entity<Contest>()
                .HasMany(c => c.PrintRequests)
                .WithRequired(p => p.Contest);

            modelBuilder.Entity<Contest>()
                .HasMany(c => c.Managers)
                .WithRequired(cm => cm.Contest);

            modelBuilder.Entity<Problem>()
                .HasMany(p => p.TestCases)
                .WithRequired(t => t.Problem);

            modelBuilder.Entity<Problem>()
                .HasMany(p => p.Clarifications)
                .WithOptional(c => c.Problem);

            modelBuilder.Entity<Problem>()
                .HasMany(p => p.Statuses)
                .WithRequired(s => s.Problem);

            modelBuilder.Entity<Solution>()
               .HasMany(s => s.SolutionTags)
               .WithRequired(st => st.Solution);

            modelBuilder.Entity<Problem>()
               .HasMany(p => p.Solutions)
               .WithRequired(s => s.Problem);

            modelBuilder.Entity<Problem>()
              .HasMany(p => p.Glances)
              .WithRequired(g => g.Problem);

            modelBuilder.Entity<Problem>()
             .HasMany(p => p.Locks)
             .WithRequired(l => l.Problem);

            modelBuilder.Entity<Status>()
                .HasMany(s => s.JudgeTasks)
                .WithRequired(j => j.Status);

            modelBuilder.Entity<Status>()
               .HasMany(s => s.Hacks)
               .WithRequired(h => h.Status);

            modelBuilder.Entity<Reply>()
                .HasMany(r => r.Replies)
                .WithRequired(r => r.Father);

            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Replies)
                .WithRequired(r => r.Topic);

            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Topics)
                .WithRequired(t => t.Forum);

            modelBuilder.Entity<Forum>()
                .HasMany(f => f.Children)
                .WithOptional(f => f.Father);

            modelBuilder.Entity<AlgorithmTag>()
                .HasMany(a => a.Children)
                .WithOptional(a => a.Father);
        }
    }
}
