USE master
GO

IF DB_ID('tenmo') IS NOT NULL
BEGIN
	ALTER DATABASE tenmo SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	DROP DATABASE tenmo;
END

CREATE DATABASE tenmo
GO

USE tenmo
GO

CREATE TABLE transfer_types (
	transfer_type_id int IDENTITY(1000,1) NOT NULL,
	transfer_type_desc varchar(10) NOT NULL,
	CONSTRAINT PK_transfer_types PRIMARY KEY (transfer_type_id)
)

CREATE TABLE transfer_statuses (
	transfer_status_id int IDENTITY(2000,1) NOT NULL,
	transfer_status_desc varchar(10) NOT NULL,
	CONSTRAINT PK_transfer_statuses PRIMARY KEY (transfer_status_id)
)

CREATE TABLE users (
	user_id int IDENTITY(3000,1) NOT NULL,
	username varchar(50) NOT NULL,
	password_hash varchar(200) NOT NULL,
	salt varchar(200) NOT NULL,
	CONSTRAINT PK_user PRIMARY KEY (user_id)
)

CREATE TABLE accounts (
	account_id int IDENTITY(4000,1) NOT NULL,
	user_id int NOT NULL,
	balance decimal(13, 2) NOT NULL,
	CONSTRAINT PK_accounts PRIMARY KEY (account_id),
	CONSTRAINT FK_accounts_user FOREIGN KEY (user_id) REFERENCES users (user_id)
)

CREATE TABLE transfers (
	transfer_id int IDENTITY(5000,1) NOT NULL,
	transfer_type_id int NOT NULL,
	transfer_status_id int NOT NULL,
	account_from int NOT NULL,
	account_to int NOT NULL,
	amount decimal(13, 2) NOT NULL,
	CONSTRAINT PK_transfers PRIMARY KEY (transfer_id),
	CONSTRAINT FK_transfers_accounts_from FOREIGN KEY (account_from) REFERENCES accounts (account_id),
	CONSTRAINT FK_transfers_accounts_to FOREIGN KEY (account_to) REFERENCES accounts (account_id),
	CONSTRAINT FK_transfers_transfer_statuses FOREIGN KEY (transfer_status_id) REFERENCES transfer_statuses (transfer_status_id),
	CONSTRAINT FK_transfers_transfer_types FOREIGN KEY (transfer_type_id) REFERENCES transfer_types (transfer_type_id),
	CONSTRAINT CK_transfers_not_same_account CHECK  ((account_from<>account_to)),
	CONSTRAINT CK_transfers_amount_gt_0 CHECK ((amount>0))
)


INSERT INTO transfer_statuses (transfer_status_desc) VALUES ('Pending');
INSERT INTO transfer_statuses (transfer_status_desc) VALUES ('Approved');
INSERT INTO transfer_statuses (transfer_status_desc) VALUES ('Rejected');

INSERT INTO transfer_types (transfer_type_desc) VALUES ('Request');
INSERT INTO transfer_types (transfer_type_desc) VALUES ('Send');

BEGIN TRANSACTION
SELECT balance, account_id, a.user_id FROM accounts a INNER JOIN users u on a.user_id = u.user_id WHERE u.user_id = 3000
ROLLBACK TRANSACTION

--get transfers where receiving
SELECT * FROM transfers t INNER JOIN accounts a ON t.account_to = a.account_id WHERE a.account_id = 4000

SELECT * FROM users
SELECT * FROM accounts

SELECT * FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id WHERE a.account_id = 4000

SELECT transfer_id AS 'transferId', transfer_type_id AS 'transferTypeId', transfer_status_id AS 'transferStatusId', account_from AS 'accountFrom', account_to AS 'accountTo', amount FROM transfers WHERE (account_from = 4000 OR account_to= 4000) 

SELECT u.user_id AS 'fromUserId' FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE a.account_id = 4000 

SELECT
 (SELECT u.user_id FROM users u INNER JOIN accounts a ON u.user_id = a.user_id INNER JOIN transfers t ON a.account_id = t.account_from WHERE (account_from = 4000 OR account_to= 4000)) AS 'fromUserId',
 (SELECT u.user_id FROM users u INNER JOIN accounts a ON u.user_id = a.user_id INNER JOIN transfers t ON a.account_id = t.account_to WHERE (account_from = 4000 OR account_to= 4000)) AS 'toUserId'
 
 --get transfers sent from account	4003			--
 SELECT  t.account_from AS 'accountFrom', t.account_to AS 'accountTo', t.amount AS 'amount', t.transfer_id AS 'transferId', t.transfer_type_id AS 'transferTypeId', tt.transfer_type_desc AS 'statusDescription',  t.transfer_status_id AS 'transferStatusId', ts.transfer_status_desc AS 'statusDescription', u.username AS 'toUserName', u.user_id AS 'toUserId' FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id --WHERE t.account_from = 4003

 --pull all data for transfer object			--
 SELECT  t.account_from AS 'accountFrom', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromUser', (SELECT u.user_id FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_from = a.account_id) AS 'fromUserId', (SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toUser', (SELECT u.user_id FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id) AS 'toUserId', t.account_to AS 'accountTo', t.amount AS 'amount', t.transfer_id AS 'transferId', t.transfer_type_id AS 'transferTypeId',  tt.transfer_type_desc AS 'typrDescription',  t.transfer_status_id AS 'transferStatusId', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE account_from = 4000 OR account_to = 4000

 --more limited query
  SELECT  t.transfer_id AS 'transferId', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromName', (SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toName', t.amount AS 'amount', tt.transfer_type_desc AS 'typeDescription', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE u.user_id = 3003;
  
  --won't have account num....
  SELECT  t.transfer_id AS 'transferId', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromName', (SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toName', t.amount AS 'amount', tt.transfer_type_desc AS 'typeDescription', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE account_from = 4000 OR account_to = 4000
  --fixing too narrow
  SELECT  t.transfer_id AS 'transferId', (SELECT u.username WHERE t.account_from = a.account_id)  AS 'fromName', (SELECT username FROM users u INNER JOIN accounts a ON u.user_id = a.user_id WHERE t.account_to = a.account_id)  AS 'toName', t.amount AS 'amount', tt.transfer_type_desc AS 'typeDescription', ts.transfer_status_desc AS 'statusDescription'  FROM transfers t INNER JOIN accounts a ON t.account_from = a.account_id INNER JOIN users u on a.user_id = u.user_id INNER JOIN transfer_statuses ts  ON t.transfer_status_id = ts.transfer_status_id INNER JOIN transfer_types tt ON t.transfer_type_id = tt.transfer_type_id WHERE (SELECT a.account_id FROM accounts a INNER JOIN users u ON a.user_id = u.user_id INNER JOIN transfers t ON a.account_id = t.account_from);


  SELECT * FROM users
  SELECT * FROM accounts
  SELECT * FROM transfers
