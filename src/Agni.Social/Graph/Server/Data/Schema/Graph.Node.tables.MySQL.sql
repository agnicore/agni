delimiter ;.
-- Table tbl_node
create table `tbl_node`
(
 `GDID`           BINARY(12)     not null,
 `TYP`            char(8)        not null comment 'Type of node such as: User, Forum, Club etc.',
 `G_OSH`          BINARY(12)     not null comment 'Origin sharding GDID',
 `G_ORI`          BINARY(12)     not null comment 'Origin GDID',
 `ONM`            varchar(48)    not null comment 'Origin string name',
 `ODT`            VARBINARY(256)  comment 'Origin data. BSON',
 `CDT`            datetime(0)    not null comment 'The UTC date of node creation',
 `FVI`            CHAR(1)        not null comment 'Default friend visibility',
 `IN_USE`         CHAR(1)        not null comment 'Logical Deletion flag',
  constraint `pk_tbl_node_primary` primary key (`GDID`)
)
    comment = 'Holds Data about Graph Node which represent various socially-addressable entities in the host system, e.g. (PROD, 123, 45, \'SONY PLAYER\');(USR, 89, 23, \'Oleg Popov\')'
;.

delimiter ;.
  create  index `idx_tbl_node_ori` on `tbl_node`(`G_ORI`) comment 'Used to locate records by origin, the type and origin shard are purposely omitted from this index';.
delimiter ;.
-- Table tbl_friendlist
create table `tbl_friendlist`
(
 `GDID`           BINARY(12)     not null,
 `G_OWN`          BINARY(12)     not null comment 'Graph node that has named list',
 `LID`            varchar(16)    not null comment 'Friend list ID, such as \'Work\', \'Family\'',
 `LDR`            varchar(64)     comment 'List description',
 `CDT`            datetime(0)    not null comment 'When was created',
  constraint `pk_tbl_friendlist_primary` primary key (`GDID`)
)
    comment = 'Friend lists per node - a list is a named set of graph node connections, such as \'Family\', \'Coworkers\' etc.'
;.

delimiter ;.
  create  index `idx_tbl_friendlist_own` on `tbl_friendlist`(`G_OWN`);.
delimiter ;.
-- Table tbl_subscribervol
create table `tbl_subscribervol`
(
 `G_OWN`          BINARY(12)     not null comment 'Owner/emitter, briefcase key',
 `G_VOL`          BINARY(12)     not null comment 'Briefcase for subscribers; generated from NODE id',
 `CNT`            BIGINT(8) UNSIGNED not null comment 'Approximate count of subscribers in briefcase',
 `CDT`            datetime(0)    not null comment 'The UTC date of volume creation',
  constraint `pk_tbl_subscribervol_primary` primary key (`G_OWN`, `G_VOL`)
)
    comment = 'Subscription volume - splits large number of subscribers into a tree of volumes each sharded separately'
;.

delimiter ;.
-- Table tbl_subscriber
create table `tbl_subscriber`
(
 `G_VOL`          BINARY(12)     not null comment 'Who emits/to whom subscribed - briefcase key',
 `G_SUB`          BINARY(12)     not null comment 'Who subscribes',
 `STP`            char(8)        not null comment 'Type of node such as: User, Forum, Club etc.; denormalized from G_Subscriber for filtering',
 `CDT`            datetime(0)    not null comment 'The UTC date of node subscription creation',
 `PAR`            VARBINARY(256)  comment 'Subscription parameters - such as level of detail',
  constraint `pk_tbl_subscriber_primary` primary key (`G_VOL`, `G_SUB`)
)
    comment = 'Holds node subscribers, sharded on G_VOL'
;.

delimiter ;.
-- Table tbl_commentvol
create table `tbl_commentvol`
(
 `G_OWN`          BINARY(12)     not null comment 'Owner/target of comment, such as: product, service',
 `G_VOL`          BINARY(12)     not null comment 'Briefcase of comment area',
 `DIM`            char(8)        not null comment 'Dimension - such as \'review\', \'qna\'; a volume BELONGS to the particular dimension',
 `CNT`            BIGINT(8) UNSIGNED not null comment 'Approximate count of messages in briefcase',
 `CDT`            datetime(0)    not null comment 'The UTC date of volume creation',
  constraint `pk_tbl_commentvol_primary` primary key (`G_OWN`, `G_VOL`)
)
    comment = 'Comment Volume - splits large number of comments into a tree of volumes each sharded in graph node area, kept separately in Comment Area'
;.

delimiter ;.
-- Table tbl_friend
create table `tbl_friend`
(
 `GDID`           BINARY(12)     not null,
 `G_OWN`          BINARY(12)     not null comment 'A friend of WHO',
 `G_FND`          BINARY(12)     not null comment 'A friend',
 `RDT`            datetime(0)    not null comment 'The UTC date friend request',
 `SDT`            datetime(0)    not null comment 'The UTC date of status',
 `STS`            CHAR(1)        not null comment '[P]ending|[A]pproved|[D]enied|[B]anned',
 `DIR`            CHAR(1)        not null comment '[I]am|[F]riend',
 `VIS`            CHAR(1)        not null comment '[A]nyone|[P]ublic|[F]riend|[T]Private',
 `LST`            varchar(204)    comment 'Friend lists comma-separated',
  constraint `pk_tbl_friend_primary` primary key (`GDID`),
  constraint `fk_tbl_friend_own` foreign key (`G_OWN`) references `tbl_node`(`GDID`)
)
    comment = 'Holds node\'s friends. The list is capped by the system at 9999 including pending request and approved friends. 16000 including banned friends'
;.

delimiter ;.
  create unique index `idx_tbl_friend_uk` on `tbl_friend`(`G_OWN`, `G_FND`);.
delimiter ;.
  create  index `idx_tbl_friend_friend` on `tbl_friend`(`G_FND`, `G_OWN`);.
delimiter ;.
-- Table tbl_noderating
create table `tbl_noderating`
(
 `G_NOD`          BINARY(12)     not null comment 'A Node',
 `DIM`            char(8)        not null comment 'Dimension',
 `CNT`            BIGINT(8) UNSIGNED not null default '0' comment 'Count of comments (even with rating 0)',
 `RTG1`           BIGINT(8) UNSIGNED not null default '0' comment 'Count rating for value 1',
 `RTG2`           BIGINT(8) UNSIGNED not null default '0' comment 'Count rating for value 2',
 `RTG3`           BIGINT(8) UNSIGNED not null default '0' comment 'Count rating for value 3',
 `RTG4`           BIGINT(8) UNSIGNED not null default '0' comment 'Count rating for value 4',
 `RTG5`           BIGINT(8) UNSIGNED not null default '0' comment 'Count rating for value 5',
 `CDT`            datetime(0)    not null comment 'The UTC date of node rating creation',
 `LCD`            datetime(0)    not null comment 'The UTC date of change rating',
  constraint `pk_tbl_noderating_primary` primary key (`G_NOD`, `DIM`)
)
    comment = 'Rating node'
;.

