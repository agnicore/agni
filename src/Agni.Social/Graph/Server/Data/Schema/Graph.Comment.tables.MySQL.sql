delimiter ;.
-- Table tbl_comment
create table `tbl_comment`
(
 `GDID`           BINARY(12)     not null,
 `G_VOL`          BINARY(12)     not null comment 'Briefcase rating message',
 `G_ATR`          BINARY(12)     not null comment 'Author',
 `G_TRG`          BINARY(12)     not null comment 'What rated',
 `DIM`            char(8)        not null comment 'Dimension',
 `ROOT`           CHAR(1)        not null comment 'Is root',
 `G_PAR`          VARBINARY(12)   comment 'This is a response to parent',
 `MSG`            varchar(1024)   comment 'Comment message',
 `DAT`            VARBINARY(256)  comment 'Comment message data. BSON',
 `CDT`            datetime(0)    not null comment 'The UTC date of volume creation',
 `LKE`            int            not null comment 'Liked',
 `DIS`            int            not null comment 'Disliked',
 `CMP`            int            not null comment 'Count of complaints',
 `PST`            CHAR(1)        not null comment 'Publication state',
 `RTG`            tinyint        not null comment 'Rating 1-5',
 `RCNT`           BIGINT(8) UNSIGNED not null comment 'Response count',
 `IN_USE`         CHAR(1)        not null comment 'Logical Deletion flag',
  constraint `pk_tbl_comment_primary` primary key (`GDID`)
)
    comment = 'Hold comments and rating'
;.

delimiter ;.
-- Table tbl_complaint
create table `tbl_complaint`
(
 `GDID`           BINARY(12)     not null,
 `G_CMT`          BINARY(12)     not null comment 'Reference to comment that complaint is for',
 `G_ATH`          BINARY(12)     not null comment 'Author graph node',
 `KND`            char(8)        not null comment 'Kind of complaint',
 `MSG`            varchar(1024)   comment 'Complaint message',
 `CDT`            datetime(0)    not null comment 'The UTC date of complaint creation',
 `IN_USE`         CHAR(1)        not null comment 'Logical Deletion flag',
  constraint `pk_tbl_complaint_primary` primary key (`GDID`)
)
;.

delimiter ;.
  create  index `idx_tbl_complaint_cmt` on `tbl_complaint`(`G_CMT`);.
