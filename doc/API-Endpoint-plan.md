# RaceDay - API Endpoint Plan

| Entity | HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Users** | **GET** | `/api/users` | Retrieves all users | Manager | None | `200 OK` - Array of User objects |
| | **GET** | `/api/users/{id}` | Retrieves user details by ID | Any (Logged in) | None | `200 OK` - User object<br>`404 Not Found` |
| | **POST** | `/api/users` | Creates a new user account | None (Public) | `{"UserID": 104, "Username": "String", "UserEmail": "String", "UserRole": "Participant"}` | `201 Created` - Created User object<br>`400 Bad Request` |
| | **PUT** | `/api/users/{id}` | Updates user information | Manager | `{"Username": "String", "UserEmail": "String", "UserRole": "String"}` | `200 OK` - Updated User object<br>`404 Not Found` |
| | **DELETE** | `/api/users/{id}` | Deletes a user account | Manager | None | `204 No Content`<br>`404 Not Found` |
| **Participants** | **GET** | `/api/participants` | Retrieves all registered participants | None (Public) | None | `200 OK` - Array of Participant objects |
| | **GET** | `/api/participants/{id}` | Retrieves participant details | None (Public) | None | `200 OK` - Participant object<br>`404 Not Found` |
| | **POST** | `/api/participants` | Registers a new participant | Participant | `{"PartID": 204, "Partname": "String", "PartCar": "String", "PartWins": 0}` | `201 Created` - Created Participant object<br>`400 Bad Request` |
| | **PUT** | `/api/participants/{id}` | Updates participant details and wins | Manager / Participant | `{"Partname": "String", "PartCar": "String", "PartWins": 5}` | `200 OK` - Updated Participant object<br>`404 Not Found` |
| | **DELETE** | `/api/participants/{id}` | Removes a participant | Manager | None | `204 No Content`<br>`404 Not Found` |
| **Fees** | **GET** | `/api/fees` | Retrieves all fee payment records | Manager | None | `200 OK` - Array of Fee objects |
| | **GET** | `/api/fees/{id}` | Retrieves fee details by FeeID | Manager / Participant | None | `200 OK` - Fee object<br>`404 Not Found` |
| | **GET** | `/api/fees/participant/{partId}` | Retrieves fee details for a participant | Manager / Participant | None | `200 OK` - Fee object<br>`404 Not Found` |
| | **POST** | `/api/fees` | Records a new fee payment entry | Manager | `{"FeeID": 304, "FeePaid": "Yes", "FeeDiscount": "No", "FeeAmount": "5500", "PartID": 201}` | `201 Created` - Created Fee object<br>`400 Bad Request` |
| | **PUT** | `/api/fees/{id}` | Updates fee payment status or amount | Manager | `{"FeePaid": "Yes", "FeeDiscount": "Yes", "FeeAmount": "5000", "PartID": 201}` | `200 OK` - Updated Fee object<br>`404 Not Found` |
| | **DELETE** | `/api/fees/{id}` | Deletes a fee record | Manager | None | `204 No Content`<br>`404 Not Found` |
| **Teams** | **GET** | `/api/teams` | Retrieves all racing teams | None (Public) | None | `200 OK` - Array of Team objects |
| | **GET** | `/api/teams/{id}` | Retrieves team details by TeamID | None (Public) | None | `200 OK` - Team object<br>`404 Not Found` |
| | **POST** | `/api/teams` | Creates a new racing team | Participant / Manager | `{"TeamID": 404, "TeamName": "String", "TeamPosition": "4th", "PartID": 201}` | `201 Created` - Created Team object<br>`400 Bad Request` |
| | **PUT** | `/api/teams/{id}` | Updates team name or ranking position | Manager | `{"TeamName": "String", "TeamPosition": "1st", "PartID": 201}` | `200 OK` - Updated Team object<br>`404 Not Found` |
| | **DELETE** | `/api/teams/{id}` | Deletes a team record | Manager | None | `204 No Content`<br>`404 Not Found` |
| **Track** | **GET** | `/api/tracks` | Retrieves all race tracks | None (Public) | None | `200 OK` - Array of Track objects |
| | **GET** | `/api/tracks/{id}` | Retrieves track details by TrackID | None (Public) | None | `200 OK` - Track object<br>`404 Not Found` |
| | **POST** | `/api/tracks` | Adds a new race track | Manager | `{"TrackID": 504, "TrackName": "String", "TrackLocation": "String", "TrackGrade": "A"}` | `201 Created` - Created Track object<br>`400 Bad Request` |
| | **PUT** | `/api/tracks/{id}` | Updates track information | Manager | `{"TrackName": "String", "TrackLocation": "String", "TrackGrade": "S"}` | `200 OK` - Updated Track object<br>`404 Not Found` |
| | **DELETE** | `/api/tracks/{id}` | Removes a track | Manager | None | `204 No Content`<br>`404 Not Found` |
| **Event** | **GET** | `/api/events` | Retrieves all scheduled racing events | None (Public) | None | `200 OK` - Array of Event objects |
| | **GET** | `/api/events/{id}` | Retrieves specific event details | None (Public) | None | `200 OK` - Event object<br>`404 Not Found` |
| | **POST** | `/api/events` | Creates a new racing event | Manager | `{"EventID": 604, "EventName": "String", "EventClass": "SS+", "TrackID": 501}` | `201 Created` - Created Event object<br>`400 Bad Request` |
| | **PUT** | `/api/events/{id}` | Updates event details or assigned track | Manager | `{"EventName": "String", "EventClass": "SS+", "TrackID": 501}` | `200 OK` - Updated Event object<br>`404 Not Found` |
| | **DELETE** | `/api/events/{id}` | Deletes an event | Manager | None | `204 No Content`<br>`404 Not Found` |
| **TeamEvent** | **GET** | `/api/team-events` | Retrieves all team-event registrations | None (Public) | None | `200 OK` - Array of TeamEvent objects |
| | **GET** | `/api/team-events/{id}` | Retrieves a team-event record by EventTeamID | None (Public) | None | `200 OK` - TeamEvent object<br>`404 Not Found` |
| | **POST** | `/api/team-events` | Enrolls a team into an event | Manager / Participant | `{"EventTeamID": 704, "TeamID": 401, "EventID": 601}` | `201 Created` - Created TeamEvent object<br>`400 Bad Request` |
| | **PUT** | `/api/team-events/{id}` | Updates team or event assignment | Manager | `{"TeamID": 401, "EventID": 602}` | `200 OK` - Updated TeamEvent object<br>`404 Not Found` |
| | **DELETE** | `/api/team-events/{id}` | Withdraws a team from an event | Manager | None | `204 No Content`<br>`404 Not Found` |