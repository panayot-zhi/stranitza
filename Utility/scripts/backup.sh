#!/bin/bash

# Define the backup date
BACKUP_DATE=$(date +%Y-%m-%d)

# Define remote server details
BACKUP_USER=panayot

# Step 1: Zip the website folder
zip -r /home/$BACKUP_USER/app/stranitza_website_$BACKUP_DATE.zip /srv/www/stranitza.eu

# Step 2: Backup the MySQL database
mysqldump --defaults-file=~/.mysql.cnf --routines stranitza > /home/$BACKUP_USER/app/stranitza_db_$BACKUP_DATE.sql

#TODO: Resolve current working directory
cd /home/$BACKUP_USER/backups/stranitza

# Step 3: Delete the oldest sql file if there are more than 10 backups
if [ $(ls --format=single-column *.sql 2>/dev/null | wc --lines) -gt 10 ]; then
    ls --format=single-column *.sql | head --lines=1 | xargs rm
fi

# Step 4: Copy over the current sql backup to the remote server
scp /home/$BACKUP_USER/app/stranitza_db_$BACKUP_DATE.sql ${BACKUP_USER}@${BACKUP_REMOTE_HOST}:${BACKUP_REMOTE_DIR}

# Step 5: Delete the oldest zip file if there are more than 10 backups
if [ $(ls --format=single-column *.zip 2>/dev/null | wc --lines) -gt 3 ]; then
    ls --format=single-column *.zip | head --lines=1 | xargs rm
fi

# Step 6: Upload to remote server using scp
scp /home/$BACKUP_USER/app/stranitza_website_$BACKUP_DATE.zip ${BACKUP_USER}@${BACKUP_REMOTE_HOST}:${BACKUP_REMOTE_DIR}