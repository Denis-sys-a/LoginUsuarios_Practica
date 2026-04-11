/* -- Crear la base de datos
CREATE DATABASE IF NOT EXISTS db_login
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
-- Usar la base de datos
USE db_login;
-- ============================================
-- Tabla principal de usuarios
-- ============================================
CREATE TABLE usuarios (
    id              INT             NOT NULL AUTO_INCREMENT,
    username        VARCHAR(50)     NOT NULL,
    password_hash   VARCHAR(255)    NOT NULL,   
    email           VARCHAR(100)    NOT NULL,
    activo          TINYINT(1)      NOT NULL DEFAULT 1,
    fecha_registro  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ultimo_acceso   DATETIME        NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_username (username),
    UNIQUE KEY uq_email    (email)
);
-- ============================================
-- Tabla de sesiones 
-- ============================================
CREATE TABLE sesiones (
    id              INT             NOT NULL AUTO_INCREMENT,
    usuario_id      INT             NOT NULL,
    token           VARCHAR(255)    NOT NULL,
    fecha_inicio    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_expira    DATETIME        NOT NULL,
    ip_address      VARCHAR(45)     NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_token (token),
    CONSTRAINT fk_sesion_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE
);
-- ============================================
-- Usuario de prueba (password: Admin123!)
-- Hash SHA256 del password
-- ============================================
INSERT INTO usuarios (username, password_hash, email)
VALUES (
    'admin',
    '0a041b9462caa4a31bac3567e0b6e6fd9100787db2ab433d96f6d178cabfce90',
    'admin@test.com'
);

select * from usuarios;
*/