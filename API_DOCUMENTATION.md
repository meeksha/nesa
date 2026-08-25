# NESA API Documentation

Base URL:

```text
http://127.0.0.1:8000
```

Interactive Swagger documentation:

```text
http://127.0.0.1:8000/docs
```

The API uses JSON for request and response data.

---

## 1. GET /hello

Checks whether the API is running.

### Request

```http
GET /hello
```

### Response

```json
{
  "message": "Hello, World!"
}
```

### Status Codes

| Code | Meaning |
|---|---|
| 200 | Successful response |

---

# 2. GET /patients

Returns patient records.

This endpoint supports searching, age filtering, and sorting.

## Query Parameters

| Parameter | Type | Required | Description |
|---|---|---:|---|
| `search` | string | No | Searches ID, name, age, and diagnosis |
| `min_age` | integer | No | Minimum age |
| `max_age` | integer | No | Maximum age |
| `sort_by` | string | No | Field to sort by |
| `sort_order` | string | No | `asc` or `desc` |

## Search

Example:

```http
GET /patients?search=fever
```

The search performs partial matching.

The backend checks:

```text
id
name
age
diagnosis
```

The search is case-insensitive for SQLite's normal text matching behavior.

## Age Range

Example:

```http
GET /patients?min_age=30&max_age=40
```

The SQL condition is:

```sql
WHERE age >= ? AND age <= ?
```

Therefore the boundaries are inclusive.

For example, an age of exactly `30` or `40` is included.

## Minimum Age Only

```http
GET /patients?min_age=30
```

Returns:

```text
age >= 30
```

## Maximum Age Only

```http
GET /patients?max_age=40
```

Returns:

```text
age <= 40
```

## Sorting

Sort by age:

```http
GET /patients?sort_by=age&sort_order=asc
```

Sort by name descending:

```http
GET /patients?sort_by=name&sort_order=desc
```

Supported fields:

```text
id
name
age
diagnosis
```

Supported directions:

```text
asc
desc
```

An invalid direction such as:

```text
seeec
```

is rejected instead of being used in the SQL query.

## Example Response

```json
[
  {
    "id": 1,
    "name": "Sameeksha",
    "age": 20,
    "diagnosis": "Fever"
  },
  {
    "id": 2,
    "name": "Bia",
    "age": 34,
    "diagnosis": "Diabetes"
  }
]
```

---

# 3. POST /patients

Creates a new patient.

## Request

```http
POST /patients
Content-Type: application/json
```

### Request Body

```json
{
  "id": 3,
  "name": "Rahul",
  "age": 28,
  "diagnosis": "Cold"
}
```

## Patient Model

```text
id          integer
name        string
age         integer
diagnosis   string
```

## Response

```json
{
  "message": "Patient added successfully"
}
```

## Status Codes

| Code | Meaning |
|---|---|
| 200 | Patient created successfully |
| 422 | Request validation failed |
| 500 | Database/server error |

---

# 4. PUT /patients/{patient_id}

Updates an existing patient.

The patient ID is supplied in the URL.

## Request

```http
PUT /patients/3
Content-Type: application/json
```

### Request Body

```json
{
  "id": 3,
  "name": "Rahul Kumar",
  "age": 29,
  "diagnosis": "Fever"
}
```

The ID in the URL determines which database record is updated.

The update modifies:

```text
name
age
diagnosis
```

## Response

```json
{
  "id": 3,
  "name": "Rahul Kumar",
  "age": 29,
  "diagnosis": "Fever"
}
```

---

# 5. DELETE /patients/{patient_id}

Deletes a patient.

## Request

```http
DELETE /patients/3
```

## Response

```json
{
  "message": "Patient deleted"
}
```

---

# 6. GET /analytics

Returns statistical information about the current patient database.

## Request

```http
GET /analytics
```

## Response

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

## Analytics Fields

### total_patients

Number of patient records.

### average_age

Average age calculated using:

```text
sum of all ages / number of patients
```

### minimum_age

Youngest patient age.

### maximum_age

Oldest patient age.

### most_common_diagnosis

Diagnosis occurring most frequently.

### diagnosis_counts

Dictionary containing the number of patients for each diagnosis.

Example:

```json
{
  "Flu": 5,
  "Diabetes": 3
}
```

### age_groups

Patients are divided into four groups:

```text
Under 18
18-30
31-50
51+
```

---

# Error Handling

## Validation Error

FastAPI/Pydantic automatically validates request parameters and request bodies.

For example, supplying a string where an integer is expected can result in:

```text
422 Unprocessable Entity
```

## Invalid Sort Order

Example:

```http
GET /patients?sort_by=age&sort_order=seeec
```

The API returns a client error because only `asc` and `desc` are allowed.

## Backend Connection Error

The WPF application handles failure to connect to the FastAPI backend and displays an appropriate message.

## Unexpected Errors

Backend operations are wrapped with exception handling and important failures are logged.

---

# Data Flow

For a normal patient request:

```text
WPF
 |
 | HTTP Request
 v
FastAPI
 |
 | SQL Query
 v
SQLite
 |
 | Database Result
 v
FastAPI
 |
 | JSON Response
 v
WPF
 |
 v
DataGrid
```

For analytics:

```text
WPF Analytics Window
        |
        | GET /analytics
        v
     FastAPI
        |
        v
     SQLite
        |
        v
 Python data processing
        |
        v
    JSON result
        |
        v
 WPF Analytics Dashboard
        |
        +---- Summary cards
        |
        +---- Diagnosis chart
        |
        +---- Age-group chart
```

# Testing Through Swagger

The easiest way to test the API during development is:

```text
http://127.0.0.1:8000/docs
```

Recommended testing order:

1. `GET /hello`
2. `GET /patients`
3. `POST /patients`
4. `GET /patients`
5. `GET /patients?search=...`
6. `GET /patients?min_age=...&max_age=...`
7. `GET /patients?sort_by=age&sort_order=asc`
8. `GET /patients?sort_by=age&sort_order=desc`
9. `PUT /patients/{patient_id}`
10. `DELETE /patients/{patient_id}`
11. `GET /analytics`

This verifies the complete CRUD, search, filtering, sorting, and analytics flow.
