# Carver
* [What is this](#what-is-this)
* [Building](#building)
* [Configuration](#configuration)
* [API](#api)
* [TODO](#todo)

## What is this?
A simple-ish token-based authentication service.  Credentialed users can log into this system, create a new persistent token, and  query to find out if an arbitrary token was actually issued by this application.

## Building
This is designed to build and run on top of dotnet core.  Using the dotnet CLI tools, run:

```
dotnet restore
```

and

```
dotnet run --project Carver.csproj
```

## Configuration
Carver requires a running postgres instance.  You can adapt carver to your postgres settings by specifying the following in an appsettings.json:

```
"db_server" (default=127.0.0.1)
"db_port" (default=5432)
"db_name" (default=carver_db)
"db_username"
"db_password"
"db_command_timeout_sec" (default=20)
"db_ssl_mode" (default="Require")
"db_pooling" (default=true)
"db_min_pool_size" (default=1)
"db_max_pool_size" (default=20)
"db_connection_lifetime_sec" (default=15)
```

Configure the interface on which carver listens by setting the following in an appsettings.json:

```
"api_host"
"api_port"
```

## API
### Getting a session token (Logging in)

SSL: Required

Permissions: user, validator, admin

Request:

```curl -X POST -F 'username=my_user_name' -F 'password=my_password' https://{carver_ip}/sessions```

Response:

* 200 -- OK -- ```{ token=your_session_token }```
* 400 -- request missing username or password

### Invalidating a session token (Logging out)

Just drop it client-side.

### Creating a user

SSL: Required

Permissions: admin

Request:

```curl -X POST https://{carver_ip}/users?api_token=my_session_token -d '{"username"="alice", "password"="p@ssword", "email"="alice@gmail.com"}'```

Response:

* 200 -- OK
* 400 -- invalid username/password/email
* 403 -- Invalid creds.  Check your api_token & permissions.

### Creating a token

SSL: Required

Permissions: user, admin

Request:

```curl -X POST https://{carver_ip}/tokens?api_token=my_session_token -d '{"description"="where_is_this_token_being_stored"}'```

Response:

* 200 -- OK -- ```{ "token":"the_token" } ```
* 403 -- Invalid creds.  Check your api_token & permissions.

### Validate a token

Permissions: validator, admin

Request:

```curl -X GET https://{carver_ip}/tokens/{token}?api_token=my_session_token```

Response:

* 200 -- Yeah that's a valid token
* 404 -- Nope not a valid token

## TODO
- Token caching
- Allow user updates
- Expire session tokens
