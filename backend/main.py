from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from database import get_connection
import logging


logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(levelname)s - %(message)s",
    force=True
)

logger = logging.getLogger("nesa")

app = FastAPI()
print("LOADED MAIN.PY FROM:")
print(__file__)


@app.on_event("startup")
async def startup_event():

    logger.info("NESA API STARTED")

class Patient(BaseModel):
    id: int
    name: str
    age: int
    diagnosis: str

@app.get("/patients")
async def get_patients(
    search: str | None = None,
    min_age: int | None = None,
    max_age: int | None = None,
    sort_by: str | None = None,
    sort_order: str = "asc"
):
    print(f"was asked to check!!")

    logger.info(
        f"Fetching patients | "
        f"search={search}, "
        f"min_age={min_age}, "
        f"max_age={max_age}, "
        f"sort_by={sort_by}, "
        f"sort_order={sort_order}"
    )

    connection = get_connection()

    try:
        cursor = connection.cursor()

        if search:

            search = search.strip()

            cursor.execute(
                """
                SELECT id, name, age, diagnosis
                FROM patients
                WHERE CAST(id AS TEXT) LIKE ?
                   OR name LIKE ?
                   OR CAST(age AS TEXT) LIKE ?
                   OR diagnosis LIKE ?
                """,
                (
                    f"%{search}%",
                    f"%{search}%",
                    f"%{search}%",
                    f"%{search}%"
                )
            )

        elif min_age is not None and max_age is not None:

            if min_age > max_age:
                raise HTTPException(
                    status_code=400,
                    detail="Minimum age cannot be greater than maximum age"
                )

            cursor.execute(
                """
                SELECT id, name, age, diagnosis
                FROM patients
                WHERE age >= ? AND age <= ?
                """,
                (min_age, max_age)
            )

        elif min_age is not None:

            cursor.execute(
                """
                SELECT id, name, age, diagnosis
                FROM patients
                WHERE age >= ?
                """,
                (min_age,)
            )

        elif max_age is not None:

            cursor.execute(
                """
                SELECT id, name, age, diagnosis
                FROM patients
                WHERE age <= ?
                """,
                (max_age,)
            )

        elif sort_by is not None:

            allowed_sort_fields = {
                "id": "id",
                "name": "name",
                "age": "age",
                "diagnosis": "diagnosis"
            }

            sort_order = sort_order.lower()

            if sort_order not in ["asc", "desc"]:
                raise HTTPException(
                    status_code=400,
                    detail="sort_order must be 'asc' or 'desc'"
                )

            if sort_by not in allowed_sort_fields:
                raise HTTPException(
                    status_code=400,
                    detail="Invalid sort field. "
                           "Use id, name, age or diagnosis."
                )

            cursor.execute(
                f"""
                SELECT id, name, age, diagnosis
                FROM patients
                ORDER BY {allowed_sort_fields[sort_by]} {sort_order}
                """
            )


        else:

            cursor.execute(
                """
                SELECT id, name, age, diagnosis
                FROM patients
                """
            )

        rows = cursor.fetchall()

        logger.info(
            f"Patients fetched successfully: {len(rows)} records"
        )

        return [
            {
                "id": row[0],
                "name": row[1],
                "age": row[2],
                "diagnosis": row[3]
            }
            for row in rows
        ]

    except HTTPException:
        raise

    except Exception as e:

        logger.error(
            f"Failed to fetch patients: {e}"
        )

        raise HTTPException(
            status_code=500,
            detail="Failed to fetch patients"
        )

    finally:
        connection.close()

@app.post("/patients")
async def add_patient(patient: Patient):

    connection = get_connection()

    try:

        cursor = connection.cursor()

        cursor.execute(
            """
            INSERT INTO patients (id, name, age, diagnosis)
            VALUES (?, ?, ?, ?)
            """,
            (
                patient.id,
                patient.name,
                patient.age,
                patient.diagnosis
            )
        )

        connection.commit()

        logger.info(
            f"Patient added successfully: "
            f"ID={patient.id}, Name={patient.name}"
        )

        return {
            "message": "Patient added successfully"
        }

    except Exception as e:

        logger.error(
            f"Failed to add patient ID={patient.id}: {e}"
        )

        raise

    finally:
        connection.close()

@app.put("/patients/{patient_id}")
async def update_patient(
    patient_id: int,
    patient: Patient
):

    connection = get_connection()

    try:

        cursor = connection.cursor()

        cursor.execute(
            """
            UPDATE patients
            SET name = ?, age = ?, diagnosis = ?
            WHERE id = ?
            """,
            (
                patient.name,
                patient.age,
                patient.diagnosis,
                patient_id
            )
        )

        if cursor.rowcount == 0:

            raise HTTPException(
                status_code=404,
                detail="Patient not found"
            )

        connection.commit()

        logger.info(
            f"Patient updated successfully: ID={patient_id}"
        )

        return patient

    except HTTPException:
        raise

    except Exception as e:

        logger.error(
            f"Failed to update patient ID={patient_id}: {e}"
        )

        raise

    finally:
        connection.close()

@app.delete("/patients/{patient_id}")
async def delete_patient(patient_id: int):

    connection = get_connection()

    try:

        cursor = connection.cursor()

        cursor.execute(
            """
            DELETE FROM patients
            WHERE id = ?
            """,
            (patient_id,)
        )

        if cursor.rowcount == 0:

            raise HTTPException(
                status_code=404,
                detail="Patient not found"
            )

        connection.commit()

        logger.info(
            f"Patient deleted successfully: ID={patient_id}"
        )

        return {
            "message": "Patient deleted"
        }

    except HTTPException:
        raise

    except Exception as e:

        logger.error(
            f"Failed to delete patient ID={patient_id}: {e}"
        )

        raise

    finally:
        connection.close()
@app.get("/analytics")
async def analytics():

    connection = get_connection()

    logger.info("Analytics requested")

    try:

        cursor = connection.cursor()

        cursor.execute(
            """
            SELECT age, diagnosis
            FROM patients
            """
        )

        rows = cursor.fetchall()

        if not rows:

            return {
                "total_patients": 0,
                "average_age": 0,
                "minimum_age": 0,
                "maximum_age": 0,
                "most_common_diagnosis": None,
                "diagnosis_counts": {},
                "age_groups": {
                    "Under 18": 0,
                    "18-30": 0,
                    "31-50": 0,
                    "51+": 0
                }
            }

       
        ages = [row[0] for row in rows]

        total_patients = len(ages)

        average_age = sum(ages) / total_patients

        minimum_age = min(ages)

        maximum_age = max(ages)

       

        diagnosis_counts = {}

        for row in rows:

            diagnosis = row[1]

            if diagnosis in diagnosis_counts:
                diagnosis_counts[diagnosis] += 1
            else:
                diagnosis_counts[diagnosis] = 1

        

        most_common_diagnosis = max(
            diagnosis_counts,
            key=diagnosis_counts.get
        )

      
        age_groups = {
            "Under 18": 0,
            "18-30": 0,
            "31-50": 0,
            "51+": 0
        }

        for age in ages:

            if age < 18:

                age_groups["Under 18"] += 1

            elif age <= 30:

                age_groups["18-30"] += 1

            elif age <= 50:

                age_groups["31-50"] += 1

            else:

                age_groups["51+"] += 1

        logger.info(
            "Analytics calculated successfully"
        )

        return {
            "total_patients": total_patients,
            "average_age": round(average_age, 2),
            "minimum_age": minimum_age,
            "maximum_age": maximum_age,
            "most_common_diagnosis": most_common_diagnosis,
            "diagnosis_counts": diagnosis_counts,
            "age_groups": age_groups
        }

    except Exception as e:

        logger.error(
            f"Failed to calculate analytics: {e}"
        )

        raise

    finally:
        connection.close()