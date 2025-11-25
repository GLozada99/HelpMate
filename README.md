# **HelpMate — Issue Tracking & Collaboration Platform**

# **Core Features**

## **1. Authentication & Users**
- Authentication with Cookies
- User roles: `SuperAdmin`, `Admin`, `Agent`, `Customer`
- User states: `Active`, `Inactive`
- Users can create/manage boards depending on their roles
- Only `SuperAdmin` and `Admin` users can create boards.

## **2. Boards**
Boards act as containers for tickets.
Each board has:
- Code (unique)
- Name
- Description
- Status (Active/Inactive)
- Creator (User)

### Board Membership Roles
- **Owner** → Full permissions
- **Agent** → Work, create and edit tickets
- **Editor** → Create and edit tickets
- **Viewer** → Read-only

All boards must have at least one owner

### Permissions Summary

| Action | Viewer | Editor | Agent | Owner |
|--------|--------|--------|--------|--------|
| View board | ✔ | ✔ | ✔ | ✔ |
| Create tickets | ✖ | ✔ | ✔ | ✔ |
| Edit tickets | ✖ | ✔ | ✔ | ✔ |
| Assign tickets | ✖ | ✖ | ✔ | ✔ |
| Manage members | ✖ | ✖ | ✖ | ✔ |
| Delete comments | ✖ | ✔ | ✖ | ✔ |

## **3. Tickets**

Tickets belong to boards and support:
- Title & description
- Status (`Backlog`, `Open`, `InProgress`, `Blocked`, `Closed`, `WontDo`)
- Priority (`Low`, `Medium`, `High`, `Critical`)
- Reporter & CreatedBy relationship
- Assignees
- Comments
- Tags
- Watchers
- Ticket history tracking

### Permission Rules
Defined in:
- `TicketRulesHelper`
- `TicketCommentRulesHelper`

Examples:

#### Creating a ticket
✔ must be board member
✔ must be active
✔ board must be active
✔ role must be `Editor`, `Agent`, or `Owner`

#### Assigning a user
✔ user must belong to board
✔ role cannot be `Viewer`

---

## **4. Ticket Comments**

### Comment Permission Rules

| Action | Viewer | Editor | Agent | Owner |
|--------|--------|--------|--------|--------|
| Add comment | ✔ | ✔ | ✔ | ✔ |
| Edit own comment | ✔ | ✔ | ✔ | ✔ |
| Edit others' comments | ✖ | ✖ | ✖ | ✖ |
| Delete own comment | ✔ | ✔ | ✔ | ✔ |
| Delete any comment | ✖ | ✔ | ✖ | ✔ |

---

## **5. Workflow**

The following tools are used in the project:
- `Docker`: To setup the development database, as well as the container for the application to run in.
- `docker-compose`: To configure the contanires, definne volumes and exposed ports.
- `Just`: To run commands.

The following commands are included:
- `deploy`: Builds the project into a docker container and starts it. Updates the database with any pending migrations.
- `db-new-migration 'migration_name'`: Creates a new migration file detecting any changes and updates the HelpMateDbContextModelSnapshot file
- `db-update`: Updates the database with any pending migrations.
- `db-reset`: Drops everything in the db and executes migrations.
- `api-run 'port'`: Runs the API on the specified port.
- `setup-logging`: Sets up the directory where logging comming from the container will be.
- `search-logs 'tracking_id'`: Search for all logs which have the provided tracking_id. 
---

## **6. Roadmap**

- Some fields which are currently implemented as enums may benefit (improve the system flexibility)
from being part of their own table:
  - `UserRole`
  - `MembershipRole`
  - `TicketPriority`.
- The following Entities exist in the DB as tables but are not being utilized:
  - `TicketHistory`: Its purpose is to record instances of changes in a ticket, so there is a traceability of
  what occurs such as: a change in status or priority, a new comment, a new assignment. 
  This will include a `/api/boards/{boardId}/tickets/{ticketId}/history` GET endpoint for the complete history of a ticket.
  - `Tag`: Its purpose is to serve as a grouping of tickets by theme, feature, or any custom dimension.
  - `TicketWatchers`: A M2M table between users and tickets which would serve as a hub for all the users that want to be notified when something of interest on a specific ticket occurs.
- A recurring job using the `Quartz` library to send emails to users based on the following occurrences:
  - Send emails to the `Reporter` of a Ticket whenever there is a change (or new comment).
  - Send emails to the `Assignee` of a Ticket whenever there is a change (or new comment).
  - Send emails all `Watchers` of a Ticket whenever there is a change (or new comment).
  - Send emails to the `Assignee` of a Ticket more than `X` days have passed without a status change or a comment from them (unless the ticket is closed).
- An `ApiLog` db table including elements like: `Path`, `Querystring`, `StatusCode`, `RequestBody`, `RequestHeaders`, `ResponseBody`, `TrackingId`, `UserId`.
  This along with an `ApiLogging` middleware would ensure traceability and reproducibility of operations.
- Integration testing through `ApiResponseFactory` to assert the complete functionality of the endpoints,
  as currently testing is divided between Service testing of the logic and Controller testing of the mapping of errors to their correct responses.
