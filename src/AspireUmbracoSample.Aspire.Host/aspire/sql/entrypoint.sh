#!/bin/bash
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to start
sleep 15s

# Set the directory containing data files
data_dir="/var/opt/data"

# Attach all databases found in the data directory
for mdf_file in "$data_dir"/*.mdf; do
  if [ -f "$mdf_file" ]; then
    db_name=$(basename "$mdf_file" .mdf)
    ldf_file="$data_dir/${db_name}_log.ldf"

    # Check if the database is already attached
    if /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SET NOCOUNT ON; IF DB_ID(N'$db_name') IS NULL PRINT 1 ELSE PRINT 0" -h -1 -W | grep -q '^1$'; then
      if [ -f "$ldf_file" ]; then
        echo "Attaching database $db_name with log file..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "
          CREATE DATABASE [$db_name]
          ON (FILENAME = '$mdf_file', NAME = '$db_name'),
          (FILENAME = '$ldf_file', NAME = '${db_name}_log')
          FOR ATTACH;"
      else
        echo "Attaching database $db_name without log file..."
        /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "
          CREATE DATABASE [$db_name]
          ON (FILENAME = '$mdf_file', NAME = '$db_name')
          FOR ATTACH_REBUILD_LOG;"
      fi
    else
      echo "Database $db_name is already attached."
    fi
  fi
done

# Read the database names from the environment variable
IFS=',' read -ra database_names <<< "$AUTO_CREATION_DATABASE_NAMES"

# Create databases that are listed in $AUTO_CREATION_DATABASE_NAMES but not found in the data directory
for db_name in "${database_names[@]}"; do
  # Remove leading/trailing whitespace
  db_name=$(echo "$db_name" | xargs)

  # Check if the database already exists
  if /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SET NOCOUNT ON; IF DB_ID(N'$db_name') IS NULL PRINT 1 ELSE PRINT 0" -h -1 -W | grep -q '^1$'; then
    echo "Creating new database $db_name..."
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "
      CREATE DATABASE [$db_name];"
  else
    echo "Database $db_name already exists, skipping..."
  fi
done

# Wait for SQL Server to terminate
wait