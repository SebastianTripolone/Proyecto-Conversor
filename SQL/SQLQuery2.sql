create database DBPrueba

go

use DBPrueba

go

create table Usuario(
IdUsuario int primary key identity,
Nombre varchar(50),
Correo varchar(50),
Clave varchar(200),
Restablecer bit,
Confirmado bit,
Token varchar(200)
)

select * from Usuario

DELETE From Usuario WHERE Correo = 'seba99vte@gmail.com'