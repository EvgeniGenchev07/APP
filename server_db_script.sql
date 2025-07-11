use eap_admin;
create table User(
id varchar(40),
absenceDays int,
name varchar(70) not null,
role tinyint not null,
email varchar(50) not null unique,
password varchar(100) not null,
constraint PK_User primary key(id));

create table Absence(
id int auto_increment,
type tinyint not null,
daysCount tinyint not null,
created date not null,
status tinyint not null,
daysTaken tinyint not null,
startDate date not null,
userId varchar(40) not null,
constraint FK_Absence_User foreign key(userId) 
references User(id),
constraint PK_Absence primary key(id));

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
additionalExpences decimal not null,
carUsagePerHundredKm float,
pricePerLiter double,
departureDate date not null,
expensesResponsibility varchar(256),
created date not null ,
userId varchar(40) not null,
constraint FK_Business_User foreign key(userId) 
references User(id),
constraint PK_BusinessTrip primary key(id));

create table HolidayDay(
    id int auto_increment,
    name varchar(30) not null,
    date date not null,
    isCustom bool not null,
    constraint PK_HolidayDay primary key(id)
);

alter table BusinessTrip
add issueId int;

alter table user
add contractDays int not null;

insert into User(
`id`, `contractDays`,`absenceDays`,`name`,`role`,`email`,`password`)
values("a49436773d0b4529b010ccb96b4a1f70",20,20,"Admin",1,"admin@eap-save.eu",md5("1a41bfc9"));

select * from user;
