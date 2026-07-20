This supports:

GET /api/readings
GET /api/readings?page=2
GET /api/readings?page=1&pageSize=10

It should reject:

GET /api/readings?page=0
GET /api/readings?pageSize=0
GET /api/readings?pageSize=5000