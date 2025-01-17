DROP USER IF EXISTS 'test'@'localhost';
DROP USER IF EXISTS 'testNoPass'@'localhost';
DROP USER IF EXISTS 'testSha2'@'localhost';
DROP USER IF EXISTS 'testNative'@'localhost';
DROP USER IF EXISTS 'testAnyhost'@'%';
CREATE USER 'test'@'localhost' identified by 'test';
GRANT ALL PRIVILEGES  ON *.*  TO 'test'@'localhost';
CREATE USER 'testNoPass'@'localhost';
CREATE USER 'testSha2'@'localhost' identified with caching_sha2_password by 'mysql';
GRANT ALL PRIVILEGES  ON *.*  TO 'testSha2'@'localhost';
CREATE USER 'testNative'@'localhost' identified with mysql_native_password by 'test';
CREATE USER 'testAnyhost'@'%' identified by 'test';
GRANT ALL PRIVILEGES  ON *.*  TO 'testAnyhost'@'%';
FLUSH PRIVILEGES;