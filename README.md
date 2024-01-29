# MongoDBTesting

Example project showing that testing can be made against a temporary
database running locally in a container could used for the unit-tests
that connect to MongoDB.

For this project I've added the following step to the workflow file
`main.yml`:

```
    - name: Create mongo container
      run: docker run -d -p 27017:27017 mongo:latest
```
