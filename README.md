# Carver
* [What is this](#what-is-this)
* [Building](#building)
* [Configuration](#configuration)
* [API](#api)

## What is this?
A simple token-based auth service. Credentialed users can create persistent tokens, and query/manage them. This was originally designed to enable offloading some authentication stuff from a pass-through nginx setup.

## Building
The project and tests are designed to build and run on top of dotnet core.  Using the dotnet CLI tools, run:

```
dotnet restore
```

and

```
dotnet run --project Carver.csproj
```

## Configuration
Carver requires a backing postgres store. Settings for that can be configured by specifying the following in an appsettings.json:

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

This also needs a local redis setup. Those settings are again in appsettings.json:
```
"redis_client_id"
"redis_password"
"redis_hostname"
"redis_port"
"redis_use_ssl"
```

Configure the interface on which carver listens by setting the following in appsettings.json:

```
"api_host"
"api_port"
```

## API
### Logging in

SSL: Required

Permissions: n/a

Request:

```curl -X POST -F 'username=my_user_name' -F 'password=my_password' https://{carver_ip}/sessions```

Response:

On 200 returns: ```{ session_token=your_session_token }```

### Invalidating your session token

SSL: Required

Permissions: n/a

Request:

```curl -X DELETE https://{carver_ip}/sessions?session_token={my_token}```

### Create user

SSL: Required

Permissions: manage-any-user

Request:

```curl -X POST https://{carver_ip}/users?session_token={my_token} -d '{"username"="alice", "password"="p@ssword", "email"="alice@gmail.com"}'```

Response:

On 200 returns, e.g.  ```{ user_id: 4 }```

### Update user

SSL: Required

Permissions: manage-any-user, manage-self-user

Request:

```curl -X POST https://{carver_ip}/users?session_token={my_token} -d '{"username"="alice", "old_password"="p@ssword", "new_password"="new_p@ssword", "email"="alice_new@gmail.com"}'```

Response:

On 200 returns, e.g. ```{ user_id: 4, username: "alice", email: "alice_new@gmail.com" }```

### Get user permissions
SSL: Required

Permissions: manage-any-user, manage-self-user

Request:

```curl -X GET https://{carver_ip}/users/{user_id}/permissions?session_token={my_token}```

Response:

On 200 returns, e.g. ```{ ["manage-self-user", "verify-token", ...] }```

### Ensure user permissions

SSL: Required

Permissions: manage-any-user

Request:

```curl -X POST https://{carver_ip}/users/{user_id}/permissions?session_token={my_token} -d '{["permission_name_1", "permission_name_2"]}'```


### Remove user permissions

SSL: Required

Permissions: manage-any-user

Request:

```curl -X DELETE https://{carver_ip}/users/{user_id}/permissions?session_token={my_token} -d '{["permission_name_1", "permission_name_2"]}'```

### Create token

SSL: Required

Permissions: create-token

Request:

```curl -X POST https://{carver_ip}/tokens?session_token={my_token} -d '{"description"="what's it for?", "expiration" = 12345}'```

* expiration is optional, in epoch_milli

Response:

On 200 returns, e.g. ```{ "token":"the_token" } ```

### Validate token
SSL: Required

Permissions: validate-token

Request:

```curl -X POST https://{carver_ip}/tokens/verify?session_token={my_token} -d '{"token"="abed-54-323d-e3lk"}'```

### Revoke token
SSL: Required

Permissions: revoke-token

Request:

```curl -X POST https://{carver_ip}/tokens/revoke?session_token={my_token} -d '{"token"="abed-54-323d-e3lk"}'```
