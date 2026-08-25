# NESA Patient Management and Analysis System

A full-stack Patient Management and Analysis System built using **C#, WPF, FastAPI, Python, SQLite, and JSON**.

The application allows users to create, view, search, filter, sort, update, and delete patient records. It also provides a patient analytics dashboard with summary statistics and charts.

## Architecture

```text
WPF / C#
    |
    | HTTP + JSON
    v
FastAPI / Python
    |
    +---- SQLite Database
    |
    +---- Python Analytics
    |
    v
JSON Response
    |
    v
WPF / C#
```

The WPF application is responsible for the user interface. FastAPI provides the backend REST API, SQLite stores patient records, and Python performs the patient-data analysis.

## Features

### Patient Management
- Add patient records
- View all patients
- Update patient records
- Delete patient records
- Select a patient from the WPF DataGrid

### Search
Patients can be searched using partial and case-insensitive matching.

Search supports:
- Patient ID
- Patient name
- Age
- Diagnosis

Example:

```text
Search: fever
```

can match:

```text
Fever
High Fever
fever symptoms
```

### Age Filtering

Patients can be filtered using:
- Minimum age
- Maximum age
- Minimum age only
- Maximum age only

Example:

```text
min_age = 30
max_age = 40
```

returns patients whose age satisfies:

```text
30 <= age <= 40
```

### Sorting

Patients can be sorted by:
- ID
- Name
- Age
- Diagnosis

Both ascending and descending order are supported.

Invalid sort directions such as `seeec` are rejected by the API instead of being inserted directly into the SQL query.

### Analytics

The backend provides:

- Total number of patients
- Average patient age
- Minimum age
- Maximum age
- Most common diagnosis
- Diagnosis-wise patient counts
- Age-group distribution

The WPF analytics window displays these results using summary cards and charts.

Age groups:

```text
Under 18
18-30
31-50
51+
```

## Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application logic |
| WPF | Desktop user interface |
| FastAPI | REST API backend |
| Python | Backend logic and analytics |
| SQLite | Database |
| JSON | Communication between frontend and backend |
| LiveCharts | Analytics charts |
| Uvicorn | FastAPI development server |

## Project Structure

A typical project structure is:

```text
nesa/
│
├── nesa/
│   ├── frontend/
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── AnalyticsWindow.xaml
│   │   └── AnalyticsWindow.xaml.cs
│   │
│   └── backend/
│       ├── main.py
│       ├── database.py
│       └── patients.db
│
└── README.md
```

The exact folder names may differ depending on the Visual Studio project configuration.

## Database

SQLite is used to persist patient information.

Patient records contain:

```text
id
name
age
diagnosis
```

The database is accessed by the FastAPI backend through the database connection helper.

Parameterized SQL queries are used for patient values:

```python
cursor.execute(
    "DELETE FROM patients WHERE id = ?",
    (patient_id,)
)
```

This avoids directly inserting user-provided values into SQL statements.

For dynamic sorting, the allowed column names are explicitly restricted before constructing the SQL statement.

## Running the Backend

Open PowerShell in the backend directory:

```powershell
cd path	o
esaackend
```

Start FastAPI using:

```powershell
python -m uvicorn main:app --reload
```

The API will normally be available at:

```text
http://127.0.0.1:8000
```

FastAPI's interactive documentation is available at:

```text
http://127.0.0.1:8000/docs
```

## Running the WPF Application

1. Open the solution in Visual Studio.
2. Make sure the FastAPI backend is running.
3. Build the solution.
4. Run the WPF application.
5. Use the main window to manage patients.
6. Open the Analytics window to view patient statistics and charts.

The WPF application communicates with:

```text
http://127.0.0.1:8000
```

## Validation and Error Handling

The FastAPI API uses Pydantic models for request validation.

Example:

```python
class Patient(BaseModel):
    id: int
    name: str
    age: int
    diagnosis: str
```

The application also handles:
- Invalid API requests
- Invalid sort order
- Database errors
- Backend connection failures
- Unexpected frontend exceptions
- Empty analytics datasets

The WPF application displays user-friendly error messages when the backend cannot be reached.

## Logging

Important backend operations are logged, including:

- Application startup
- Patient creation
- Patient updates
- Patient deletion
- Analytics requests
- Database/API failures

Example log:

```text
2026-08-23 23:27:46 - INFO - NESA API STARTED
2026-08-23 23:27:46 - INFO - Patient added successfully: ID=1, Name=Sameeksha
```

## API

The main endpoints are:

```text
GET    /hello
GET    /patients
POST   /patients
PUT    /patients/{patient_id}
DELETE /patients/{patient_id}
GET    /analytics
```

Detailed API information is available in:

```text
API_DOCUMENTATION.md
```

## Example Analytics Response

```json
{
  "total_patients": 2,
  "average_age": 39,
  "minimum_age": 33,
  "maximum_age": 45,
  "most_common_diagnosis": "Flu",
  "diagnosis_counts": {
    "Flu": 1,
    "Fever": 1
  },
  "age_groups": {
    "Under 18": 0,
    "18-30": 0,
    "31-50": 2,
    "51+": 0
  }
}
```

## Development Notes

The project follows a simple separation of responsibilities:

- **WPF/C#** handles presentation and user interaction.
- **FastAPI/Python** handles HTTP requests and application logic.
- **SQLite** handles persistent storage.
- **Python analytics** processes patient data and produces statistical results.
- **JSON** is used as the data-exchange format.

This separation makes it possible to modify the user interface without changing the database logic and to extend the backend without rewriting the WPF interface.

## Future Improvements

Possible extensions include:

- Authentication and authorization
- More detailed patient profiles
- Exporting patient data to CSV/PDF
- Additional analytics
- Better database constraints
- Unit and integration tests
- Dependency injection
- Separate service/repository layers
- Production deployment
