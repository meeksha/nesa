import sqlite3

DATABASE = 'patients.db'
def get_connection():
    return sqlite3.connect(DATABASE,timeout=10)
def create_table():
    connection = get_connection()
    cursor=connection.cursor()
    cursor.execute('''CREATE TABLE IF NOT EXISTS patients
(id INTEGER PRIMARY KEY, name TEXT NOT NULL, age INTEGER NOT NULL, diagnosis TEXT NOT NULL)''')
    connection.commit()
    connection.close()

create_table()

