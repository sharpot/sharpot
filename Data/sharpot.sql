DROP TABLE IF EXISTS "Account";
CREATE TABLE [Account] (
    [Id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    [Name] text NOT NULL COLLATE NOCASE UNIQUE,
    [Password] text NOT NULL
);
DROP TABLE IF EXISTS "MapItem";
CREATE TABLE [MapItem] (
    [X] integer NOT NULL,
    [Y] integer NOT NULL,
    [Z] integer NOT NULL,
    [StackPosition] integer NOT NULL,
    [Id] integer NOT NULL,
    [Extra] integer NOT NULL,
    CONSTRAINT [PK_MapItem] PRIMARY KEY ([X], [Y], [Z], [StackPosition])
);
DROP TABLE IF EXISTS "MapTile";
CREATE TABLE [MapTile] (
    [X] integer NOT NULL,
    [Y] integer NOT NULL,
    [Z] integer NOT NULL,
    [GroundId] integer NOT NULL,
    CONSTRAINT [PK_MapTile] PRIMARY KEY ([X], [Y], [Z])
);
DROP TABLE IF EXISTS "Player";
CREATE TABLE [Player] (
    [Id] integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    [AccountId] integer NOT NULL,
    [Name] text NOT NULL COLLATE NOCASE UNIQUE,
    [Gender] integer NOT NULL,
    [Vocation] integer NOT NULL DEFAULT 0,
    [Level] integer NOT NULL DEFAULT 1,
    [MagicLevel] integer NOT NULL DEFAULT 0,
    [Experience] integer NOT NULL DEFAULT 0,
    [MaxHealth] integer NOT NULL DEFAULT 100,
    [MaxMana] integer NOT NULL DEFAULT 100,
    [Capacity] integer NOT NULL DEFAULT 0,
    [OutfitLookType] integer NOT NULL DEFAULT 128,
    [OutfitHead] integer NOT NULL DEFAULT 0,
    [OutfitBody] integer NOT NULL DEFAULT 0,
    [OutfitLegs] integer NOT NULL DEFAULT 0,
    [OutfitFeet] integer NOT NULL DEFAULT 0,
    [OutfitAddons] integer NOT NULL DEFAULT 0,
    [LocationX] integer,
    [LocationY] integer,
    [LocationZ] integer,
    [Direction] integer,
    CONSTRAINT [FK_Player_0] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id])
);
