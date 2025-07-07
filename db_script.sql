drop database EAPDigitalIntegrationDb;
create database EAPDigitalIntegrationDb;
use EAPDigitalIntegrationDb;
create table User(
id varchar(40),
absenceDays int not null,
name varchar(70) not null,
role tinyint not null,
email varchar(50) not null unique,
password varchar(100) not null,
constraint PK_User primary key(id));

insert into User(
`id`,`absenceDays`,`name`,`role`,`email`,`password`)values("dawoiudhawudhawuai",20,"Test",1,"example@gmail.com",md5("usercho"));

create table Absence(
id int auto_increment,
type tinyint not null,
daysCount int not null,
created date not null,
status tinyint not null,
startDate date not null,
userId varchar(40) not null,
constraint FK_Absence_User foreign key(userId) 
references User(id),
constraint PK_Absence primary key(id));

/*create table User_Absence(
userId int not null,
absenceId int not null,
constraint FK_UserId foreign key(userId) 
references User(id),
constraint FK_AbsenceId foreign key(absenceId) 
references Absence(id),
constraint PK_User_Absence primary key(userId,absenceId));*/
create table BusinessTrip(
id int auto_increment,
status tinyint not null,
issueDate date not null,
projectName varchar(60) not null,
userFullName varchar(70) not null,
task varchar(256),
startDate date not null,
endDate date not null,
totalDays tinyInt not null,
carOwnerShip tinyint not null,
wage decimal not null,
accomodationMoney decimal not null,
carBrand varchar(20),
carRegistrationNumber varchar(8),
carTripDestination varchar(20) not null,
dateOfArrival date not null,
carModel varchar(15) not null,
carUsagePerHundredKm float,
pricePerLiter double,
departureDate date not null,
expensesResponsibility varchar(256),
created date not null ,
userId varchar(40) not null,
constraint FK_Business_User foreign key(userId) 
references User(id),
constraint PK_BusinessTrip primary key(id));
select * from User;
/*create table User_BusinessTrip(
userId int not null,
businessTripId int not null,
constraint FK_BusinessTrip_User foreign key(userId) 
references User(id),
constraint FK_BusinessTrip foreign key(businessTripId) 
references BusinessTrip(id),
constraint PK_User_BusinessTrip primary key(userId,businessTripId));*/
