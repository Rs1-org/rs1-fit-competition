using Competition.Application.Helpers;
using Competition.Domain.Entities;
using Competition.Domain.Entities.FormationEntities;
using Competition.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Competition.Infrastructure.Data.Seed;

/// <summary>
/// COMP-111: players, managers, referees, formations and history for the demo selections.
/// Clubs and countries are the ones already seeded. A second run inserts nothing.
/// </summary>
public static class PersonFormationSeed
{
    private static readonly string[] TeamNames =
    [
        "FK Sarajevo", "FK Željezničar", "NK Čelik Zenica", "FK Velež Mostar",
        "HŠK Zrinjski Mostar", "FK Tuzla City", "FK Borac Banja Luka", "NK Široki Brijeg",
    ];

    private static readonly string[] FirstNames =
        ["Amar", "Haris", "Kenan", "Tarik", "Adnan", "Emir", "Faruk", "Goran", "Ivan", "Jasmin", "Luka", "Nermin"];

    private static readonly string[] LastNames =
        ["Hodžić", "Kovač", "Begić", "Petrović", "Tomić", "Marić", "Babić", "Jurić"];

    private static readonly (string First, string Last)[] Managers =
    [
        ("Sergej", "Barbarez"), ("Amar", "Osim"), ("Ivo", "Ištuk"), ("Adnan", "Čustović"),
        ("Feđa", "Dudić"), ("Samir", "Adanalić"), ("Vinko", "Marinović"), ("Toni", "Karačić"),
    ];

    private static readonly (string First, string Last)[] Referees =
    [
        ("Ognjen", "Valjić"), ("Irfan", "Peljto"), ("Elvis", "Mujčinović"), ("Luka", "Bilobrk"),
    ];

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        var selections = await db.Selections.Where(x => TeamNames.Contains(x.Name)).ToListAsync(cancellationToken);
        if (selections.Count == 0) return;

        var goalkeeper = await EnsurePosition(db, "Goalkeeper", "Goalkeeper", cancellationToken);
        var defender = await EnsurePosition(db, "Defender", "Centre-back", cancellationToken);
        var midfielder = await EnsurePosition(db, "Midfielder", "Central midfielder", cancellationToken);
        var forward = await EnsurePosition(db, "Forward", "Striker", cancellationToken);
        Position[] byShirt = [goalkeeper, defender, defender, defender, defender, midfielder, midfielder, midfielder, midfielder, forward, forward, forward];

        await EnsureFormation(db, "4-3-3", 4, 3, 3, cancellationToken);
        await EnsureFormation(db, "4-4-2", 4, 4, 2, cancellationToken);

        var players = await db.Players.Select(x => new { x.Id, x.FirstName, x.LastName }).ToListAsync(cancellationToken);
        var knownPlayers = players.Select(x => Key(x.FirstName, x.LastName)).ToHashSet();
        var linkedPlayers = await db.PlayerSelectionHistories
            .Where(x => x.SelectionId != null)
            .Select(x => new { x.PlayerId, x.SelectionId })
            .ToListAsync(cancellationToken);
        var playerLinks = linkedPlayers.Select(x => $"{x.PlayerId:N}|{x.SelectionId:N}").ToHashSet();
        var valued = (await db.TransferValueHistories.Select(x => x.PlayerId).ToListAsync(cancellationToken)).ToHashSet();

        for (var team = 0; team < TeamNames.Length; team++)
        {
            var selection = selections.FirstOrDefault(x => x.Name == TeamNames[team]);
            if (selection == null) continue;
            for (var shirt = 1; shirt <= 12; shirt++)
            {
                var first = FirstNames[shirt - 1];
                var last = LastNames[team];
                var position = byShirt[shirt - 1];
                var sub = position.SubPositions.First();
                Player player;
                if (!knownPlayers.Contains(Key(first, last)))
                {
                    player = new Player
                    {
                        FirstName = first, LastName = last,
                        DateOfBirth = new LocalDate(1996 + (shirt % 8), 3, team + 1),
                        Picture = [], Height = 176 + shirt, ShirtNumber = shirt,
                        TransferValue = 200_000 + shirt * 25_000,
                        Position = position, SubPosition = sub,
                    };
                    db.Players.Add(player);
                    knownPlayers.Add(Key(first, last));
                }
                else
                {
                    var existingId = players.First(x => x.FirstName == first && x.LastName == last).Id;
                    player = (await db.Players.FirstAsync(x => x.Id == existingId, cancellationToken));
                }

                var link = player.Id == Guid.Empty ? null : $"{player.Id:N}|{selection.Id:N}";
                if (link == null || !playerLinks.Contains(link))
                {
                    db.PlayerSelectionHistories.Add(new PlayerSelectionHistory
                    {
                        Player = player, Selection = selection, ShirtNumber = shirt,
                        MatchPlayed = 10, Goals = shirt % 6, Assists = shirt % 4,
                    });
                    if (link != null) playerLinks.Add(link);
                }

                if (player.Id == Guid.Empty || !valued.Contains(player.Id))
                {
                    db.TransferValueHistories.Add(new TransferValueHistory
                    {
                        Player = player, Date = new LocalDate(2025, 7, 1), Value = 200_000 + shirt * 25_000,
                    });
                    if (player.Id != Guid.Empty) valued.Add(player.Id);
                }
            }
        }

        await SeedStaff(db, selections, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedStaff(AppDbContext db, List<Selection> selections, CancellationToken cancellationToken)
    {
        var managers = await db.Managers.Select(x => new { x.Id, x.FirstName, x.LastName }).ToListAsync(cancellationToken);
        var knownManagers = managers.Select(x => Key(x.FirstName, x.LastName)).ToHashSet();
        var managerLinks = (await db.SelectionManagerHistories
            .Where(x => x.SelectionId != null)
            .Select(x => new { x.ManagerId, x.SelectionId })
            .ToListAsync(cancellationToken))
            .Select(x => $"{x.ManagerId:N}|{x.SelectionId:N}").ToHashSet();

        for (var team = 0; team < Managers.Length; team++)
        {
            var selection = selections.FirstOrDefault(x => x.Name == TeamNames[team]);
            if (selection == null) continue;
            var (first, last) = Managers[team];
            Manager manager;
            if (!knownManagers.Contains(Key(first, last)))
            {
                manager = new Manager
                {
                    FirstName = first, LastName = last, DateOfBirth = new LocalDate(1978, 6, team + 1),
                    Picture = [], CareerStart = new LocalDate(2008, 7, 1),
                };
                db.Managers.Add(manager);
            }
            else
            {
                var id = managers.First(x => x.FirstName == first && x.LastName == last).Id;
                manager = await db.Managers.FirstAsync(x => x.Id == id, cancellationToken);
            }

            if (manager.Id != Guid.Empty && managerLinks.Contains($"{manager.Id:N}|{selection.Id:N}"))
                continue;
            db.SelectionManagerHistories.Add(new SelectionManagerHistory
            {
                Manager = manager, Selection = selection, Wins = 8 + team, Draw = 4, Loses = 3,
            });
        }

        var referees = (await db.Referees.Select(x => x.FirstName + "|" + x.LastName).ToListAsync(cancellationToken)).ToHashSet();
        foreach (var (index, referee) in Referees.Select((item, index) => (index, item)))
        {
            if (referees.Contains(Key(referee.First, referee.Last))) continue;
            db.Referees.Add(new Referee
            {
                FirstName = referee.First, LastName = referee.Last,
                DateOfBirth = new LocalDate(1984, 4, index + 1),
                Picture = [], CareerStart = new LocalDate(2012, 8, 1),
            });
        }
    }

    private static async Task<Position> EnsurePosition(AppDbContext db, string name, string subName, CancellationToken cancellationToken)
    {
        var slug = SlugHelper.GenerateSlug(name);
        var position = await db.Positions.Include(x => x.SubPositions).FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (position == null)
        {
            position = new Position { Name = name, Slug = slug };
            db.Positions.Add(position);
        }

        var subSlug = SlugHelper.GenerateSlug(subName);
        if (position.SubPositions.All(x => x.Slug != subSlug))
            position.SubPositions.Add(new SubPosition { Name = subName, Slug = subSlug, Position = position });
        return position;
    }

    private static async Task EnsureFormation(AppDbContext db, string code, int defenders, int midfielders, int attackers, CancellationToken cancellationToken)
    {
        if (await db.Formations.IgnoreQueryFilters().AnyAsync(x => x.Slug == code || x.Code == code, cancellationToken)) return;
        var mark = await db.FormationMarks.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Slug == code, cancellationToken);
        if (mark == null)
        {
            mark = new FormationMark { Defenders = defenders, Midfielders = midfielders, Attackers = attackers, Slug = code };
            db.FormationMarks.Add(mark);
            await db.SaveChangesAsync(cancellationToken);
        }
        if (await db.Formations.IgnoreQueryFilters().AnyAsync(x => x.FormationMarkId == mark.Id, cancellationToken)) return;
        db.Formations.Add(new Formation { Code = code, Slug = code, FormationMarkId = mark.Id });
    }

    private static string Key(string first, string last) => $"{first}|{last}";
}
