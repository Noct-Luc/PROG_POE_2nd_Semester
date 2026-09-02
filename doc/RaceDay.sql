Create database RaceDay;
Use RaceDay;

Create table Users(
UserID int primary key not null,
Username varchar(20) not null,
UserEmail varchar(100) not null,
UserRole varchar(20) not null
);

create table Participants(
PartID int primary key not null,
Partname varchar(20) not null,
PartCar varchar(30) not null,
PartWins int not null
);

create table Fees(
FeeID int primary key not null,
FeePaid varchar(10) not null,
FeeDiscount varchar(20) not null,
FeeAmount varchar(20)not null,
PartID int
Foreign key(PartID) References Participants(PartID)
);

Create table Teams(
TeamID int primary key not null,
TeamName varchar(20) not null,
TeamPosition varchar(20),
PartID int not null
Foreign key (PartID) References Participants(PartID)
);

Create table Track(
TrackID int primary key not null,
TrackName varchar(30) not null,
TrackLocation varchar(30) Not null,
TrackGrade varchar(10)
);

Create table Event(
EventID int primary key not null,
EventName varchar(50) not null,
EventClass varchar (20) not null,
TrackID int,
Foreign key(TrackID) References Track(TrackID)
);

Create table TeamEvent(
EventTeamID int primary key,
EventID int,
TeamID int,
Foreign key(TeamID) References Teams(TeamID),
Foreign key(EventID) References Event(EventID)
);

insert into Users (UserID,Username,UserEmail,UserRole) values
('101','Trash','RGB@gmail.com','Manager'),
('102','Gayter','RPB@gmail.com','Participant'),
('103','Ruin','RYB@gmail.com','Participant');

Insert into Participants (PartID,Partname,PartCar,PartWins) values
('201', 'LewisHam', 'Porche V20', '3'),
('202', 'LlyodBan', 'Audi R8', '5'),
('203', 'LlyodFro', 'Mercedes M16', '5');

insert into Fees (FeeID, FeePaid,FeeDiscount,FeeAmount,PartID) values
('301', 'Yes', 'No', '5500','202'),
('302', 'Yes', 'No', '5500','203'),
('303', 'No', 'Yes', '5000','201');

Insert into Teams(TeamID,TeamName,TeamPosition,PartID) values
('401', 'Avengers', '3rd','201'),
('402', 'SSS', '2nd','202'),
('403', 'Frontera County', '1st','203');

Insert into Track (TrackID,TrackName,TrackLocation,TrackGrade) values
('501', 'Abu boom boom', 'Explosion Duchy', 'C'),
('502', 'Crossbell', 'CrossBell State', 'A'),
('503', 'Eden race course', 'Gran Pulse', 'B');

Insert into Event(EventID,EventName,EventClass,TrackID) values
('601', 'Heinz Exhibition', 'S', '501'),
('602', 'Crossbell Grand Prix', 'SS', '502'),
('603', 'Magentano League', 'SS+', '503');

Insert into TeamEvent(EventTeamID,TeamID,EventID) values
('701', '403', '603'),
('702', '401', '602'),
('703', '402', '603');

select * from Users
select * from Participants
select * from Fees
select * from Teams
select * from Track
select * from Event
select * from TeamEvent