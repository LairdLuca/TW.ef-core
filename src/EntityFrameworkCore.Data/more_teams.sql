/*
	Adding more teams 
	Change date() to GetDate() for SQL server
*/

insert into teams(name, CreatedDate) 
values 
('Chelsea F.C.',date()),
('Real Madrid',date()),
('Benfica',date()),
('Inter Milan',date()),
('Inter Miami',date()),
('Seba United',date())


/*
	After added Relathionship Coach and League
*/

insert into teams(name, CreatedDate, ModifiedDate, LeagueId, CoachId) 
values 
('Chelsea F.C.',date(),date(), 1, 1),
('Real Madrid',date(),date(), 1, 2),
('Benfica',date(),date(), 1, 3)