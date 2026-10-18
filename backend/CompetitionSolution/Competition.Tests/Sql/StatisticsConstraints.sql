-- Run only against an isolated migrated PostgreSQL test database. Fixtures are rolled back.
\set ON_ERROR_STOP on
BEGIN;
DO $$
DECLARE
    country uuid := gen_random_uuid();
    geo uuid := gen_random_uuid();
    city uuid := gen_random_uuid();
    stadium uuid := gen_random_uuid();
    status uuid := gen_random_uuid();
    selection uuid := gen_random_uuid();
    match_id uuid := gen_random_uuid();
    statistic uuid := gen_random_uuid();
    team uuid := gen_random_uuid();
BEGIN
    INSERT INTO "Countries" ("Id","Name","Slug","PublicId","CreatedAt","ModifiedAt")
    VALUES (country,'Constraint test','constraint-' || country,replace(country::text,'-',''),now(),now());
    INSERT INTO "GeoDatas" ("Id","Latitude","Longitude","PublicId","CreatedAt","ModifiedAt")
    VALUES (geo,0,0,replace(geo::text,'-',''),now(),now());
    INSERT INTO "Cities" ("Id","Name","Slug","Population","IsCapital","GeoDataId","CountryId","PublicId","CreatedAt","ModifiedAt")
    VALUES (city,'Constraint test','constraint-' || city,0,false,geo,country,replace(city::text,'-',''),now(),now());
    INSERT INTO "Stadiums" ("Id","Name","Slug","Capacity","DateOfConstruction","GeoDataId","CityId","PublicId","CreatedAt","ModifiedAt")
    VALUES (stadium,'Constraint test','constraint-' || stadium,0,now(),geo,city,replace(stadium::text,'-',''),now(),now());
    INSERT INTO "MatchStatuses" ("Id","Name","PublicId","CreatedAt","ModifiedAt")
    VALUES (status,'Constraint test',replace(status::text,'-',''),now(),now());
    INSERT INTO "Selections" ("Id","Name","Abbreviation","FollowersCount","CountryId","PublicId","CreatedAt","ModifiedAt")
    VALUES (selection,'Constraint test','TST',0,country,replace(selection::text,'-',''),now(),now());
    INSERT INTO "Matches" ("Id","Date","FollowersCount","IsPostponed","StatusId","StadiumId","PublicId","CreatedAt","ModifiedAt")
    VALUES (match_id,now(),0,false,status,stadium,replace(match_id::text,'-',''),now(),now());
    INSERT INTO "Statistics" ("Id","MatchId","PublicId","CreatedAt","ModifiedAt")
    VALUES (statistic,match_id,replace(statistic::text,'-',''),now(),now());
    INSERT INTO "TeamStatistics" ("Id","StatisticId","SelectionId","TotalShots","ShotsOnTarget","TotalRedCards","TotalYellowCards","Corners","BigChances","Possession","PublicId","CreatedAt","ModifiedAt")
    VALUES (team,statistic,selection,12,5,0,2,4,3,60,replace(team::text,'-',''),now(),now());

    BEGIN
        INSERT INTO "Statistics" ("MatchId","PublicId","CreatedAt","ModifiedAt")
        VALUES (match_id,replace(gen_random_uuid()::text,'-',''),now(),now());
        RAISE EXCEPTION 'Duplicate active match statistic was accepted';
    EXCEPTION WHEN unique_violation THEN NULL;
    END;
    BEGIN
        INSERT INTO "TeamStatistics" ("StatisticId","SelectionId","TotalShots","ShotsOnTarget","TotalRedCards","TotalYellowCards","Corners","BigChances","Possession","PublicId","CreatedAt","ModifiedAt")
        VALUES (statistic,selection,0,0,0,0,0,0,40,replace(gen_random_uuid()::text,'-',''),now(),now());
        RAISE EXCEPTION 'Duplicate active team statistic was accepted';
    EXCEPTION WHEN unique_violation THEN NULL;
    END;
    BEGIN
        UPDATE "TeamStatistics" SET "Corners" = -1 WHERE "Id" = team;
        RAISE EXCEPTION 'Negative counter was accepted';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    BEGIN
        UPDATE "TeamStatistics" SET "ShotsOnTarget" = 13 WHERE "Id" = team;
        RAISE EXCEPTION 'Shots on target above total shots was accepted';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    BEGIN
        UPDATE "TeamStatistics" SET "Possession" = 101 WHERE "Id" = team;
        RAISE EXCEPTION 'Possession above 100 was accepted';
    EXCEPTION WHEN check_violation THEN NULL;
    END;
    BEGIN
        DELETE FROM "Statistics" WHERE "Id" = statistic;
        RAISE EXCEPTION 'Hard delete with dependent team statistics was accepted';
    EXCEPTION WHEN foreign_key_violation THEN NULL;
    END;

    UPDATE "TeamStatistics" SET "DeletedAt" = now() WHERE "Id" = team;
    INSERT INTO "TeamStatistics" ("StatisticId","SelectionId","TotalShots","ShotsOnTarget","TotalRedCards","TotalYellowCards","Corners","BigChances","Possession","PublicId","CreatedAt","ModifiedAt")
    VALUES (statistic,selection,0,0,0,0,0,0,40,replace(gen_random_uuid()::text,'-',''),now(),now());
    UPDATE "Statistics" SET "DeletedAt" = now() WHERE "Id" = statistic;
    INSERT INTO "Statistics" ("MatchId","PublicId","CreatedAt","ModifiedAt")
    VALUES (match_id,replace(gen_random_uuid()::text,'-',''),now(),now());
    RAISE NOTICE 'Statistics PostgreSQL constraints passed';
END $$;
ROLLBACK;
