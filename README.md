# Uinsure Policy Service Test

## Assumptions

To run this service you will need docker desktop installed and running.

## Run the Acceptance Tests

There are two profiles in visual studio. Docker Compose and Docker Compose Without Debugging. 

Docker Compose Without Debugging will allow you to run the acceptance tests from visual studio as the debugger will be attached to the test project.

Docker Compose will allow you to debug the running Api but you will need to run the tests from the command line or hit the api via postman or swagger.

## View the database

You can access the SQL database via:

connection: localhost,1433

username: sa

password: Your_strong_password123!