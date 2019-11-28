# Meet API samples

This repository contains samples of how to integrate your application and/or service with Rest API.

So far we have sample integation code for:

- [CSharp](https://github.com/meet-rs-api/api-docs-samples/tree/master/csharp/MeetApiSample)
- [NodeJS](https://github.com/meet-rs-api/api-docs-samples/tree/master/nodejs)

We plan to add more languages and use cases as the time comes - contributions are also very welcome :)

## General overview of the Meet API integration

Meet API is hosted on the **https://api.meet.rs** address.

In general, completing a scenario on Meet API requires two steps:

- Obtaining a valid API bearer token
- Performing action while using that token

### Table of Contents

Here are the scenarios which we will cover here:

- [Authentication](#authentication)
- [Creating a Meet](#creating-a-meet)
- [Meet participants](#meet-participants)
- Meet addon configuration
- [Meet scheduling] (#meet-scheduling)
- Working with Meet projects (aka "interview job positions") 
- Manage tenants
- Webhooks

## Authentication

Meet API authentication is a standard client credentials OAUth scenario where you exchange your api key and secret for a JWT bearer token.

You can obtain your API key and secret, in Meet Pro app (<https://meet.rs/pro)> under Settings >> Company profile.

![alt text](https://meet-cdn.azureedge.net/assets/docs/settings_devtools.png "DevTools")

Using the language/technology of your choice you need to simply make this request

```pseudo
POST https://api.meet.rs/v1/token
Content-Type: application/json

{
    grant_type: "client_credentials",
    client_key: "YOUR API KEY HERE",
    client_secret: "YOUR API SECRET HERE",
}
```

Note: If your api key and secret are not valid you are going to get a **401 - Unauthorized** response status code.

In case api key and secret are valid you will get back response containing the JWT bearer access token and a timestamp when issued token will expire.

```pseudo

    // JWT bearer token
    access_token: string

    // Unix epoch based date/time when become invalid
    expires_at: number
```

Note: You can examine the content of JWT access token if you just paste it in https://jwt.io 

Ok, so you got yourself a brand new Meet API token - grroovy! 

Let us see it in action!

## Creating a Meet

### Create a Quick Meet

The simplest way create a quick Meet (60 min, default config, anyone with a link can join at anytime) is to make a simple POST request with empty body payload.

```pseudo
POST https://api.meet.rs/v1/meetings
Content-Type: application/json

Headers
---------------------------------------------
Authorization: bearer ACCESS_TOKEN_VALUE_HERE

{} <-- empty body 

```

In response of that request you will get a new Meet definition which among other things will contain the url for the newly minted Meet

```pseudo

    ...

    // A full url to a new Meet
    joinUrl: string

    ...

```

You can now give this URL to your own users so they have their frictionless Meet

### Customizing basic Meet attributes (title, description etc)

Let say you would like to customize Meet tile or description and the way to do that is to simply pass it in Meet creation request body

```pseudo
POST https://api.meet.rs/v1/meetings
Content-Type: application/json

Headers
---------------------------------------------
Authorization: bearer ACCESS_TOKEN_VALUE_HERE

{ 
    "title": "Meeting with John Snow",
    "description": "Annual gathering of the Black Crow society"
}


In response of that request you will get a new Meet definition which will have a part

```pseudo

    ...

  "code": "5LM7fdZ0",
  "joinUrl": "https://meet.rs/5LM7fdZ0",
  "description": "Annual gathering of the Black Crow society",
  "title": "Meeting with John Snow",

    ...

```

## Meet participants

If you don't provide the list of participants in the Meet creation, anyone with a link will be able to enter the Meet which in many cases is not a desirable thing.
In addition to the security aspect, many Meet scenarios require differentiation on what participants can do in the Meet.

For example, in an interview scenario typically there are roles of Interviewer and Candidate where Interviewer is driving the Meet and has access to the task library etc. while the Candidate is seeing Candidate specific experience.

For this type of scenarios, Meet API supports defining the participants which can access the Meet and their roles in a very simple but powerful way.

Every participant can authenticate himself/herself on one of the few supported ways:
  
- passcode  
- OAuth
- SMS (soon)
- ? (stackoverflow, github, amazon, twitter...) <- if requested

### Passcode authorized participants

Let start with the example mentioned above where we would like a Meet for two participants where Jack Sparrow is an interviewer and and John Smith is the candidate.

```pseudo
POST https://api.meet.rs/v1/meetings
Content-Type: application/json

Headers
---------------------------------------------
Authorization: bearer ACCESS_TOKEN_VALUE_HERE

{
  participants: [
    {
      user : {
        firstName: "Jack",
        lastName: "Sparrow",
        email: "jack@sparrow.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Passcode",
          condition: "12345"
        }
      ]
    },
    {
      user : {
        firstName: "John",
        lastName: "Smith",
        email: "john@smith.com"
      },
      role: "User",
      accessRules: [
        {
          provider: "Passcode",
          condition: "112233"
        }
      ]
    }
  ]
}

```

As you can tell from this sample in interview use case roles are mapped like:

- role: Admin       -> recruiter/hr/organizer
- role: PowerUser   -> interviewer
- role: User        -> candidate

Also, we will use the email address provided here to send meeting participants emails containing information on how and when to Meet and also a reminder email one hour before Meet starts.

### OAuth authorized participants

In this example, to make things a bit more interesting lets have a Meet of more then 2 people:

- John Smith candidate who is still going to authenticate using passcode

- Jack Sparrow interviewer who we expect to authenticate with his G Suite email jack@sparrow.com using Google authentication
- Zumi Zami interviewer who we expect to login using her Office 365 login with her email office365@zami.com

Here is how would Meet creation request look like in this scenario:

```pseudo

{
  participants: [
    {
      user : {
        firstName: "John",
        lastName: "Smith",
        email: "john@smith.com"
      },
      role: "User",
      accessRules: [
        {
          provider: "Passcode",
          condition: "112233"
        }
      ]
    },
    {
      user : {
        firstName: "Jack",
        lastName: "Sparrow",
        email: "jack@sparrow.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Google"
        }
      ]
    },
    {
      user : {
        firstName: "Zumi",
        lastName: "Zumi",
        email: "zumi@zumi.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Microsoft",
          condition: "office365@zumi.com"
        }
      ]
    }
  ]
}

  ```

> NB: In case of Zumi Zumi participant our access rule contains both provider and condition to be used while in case of Jack Sparrow there is only provider. The reason for this is that in case condition email is the same as user email, condition can be ommited.

How would this Meet work when defined like this?

Anyone opening meeting url will see this on their login page

![alt text](https://meet-cdn.azureedge.net/assets/docs/multi_auth_provider.png "Authentication with multiple providers")

John will enter the passcode he will receive in his email and join the Meet as candidate

Jack will click on "Sign in with Google" and use his work G Suite account to join the Meet.
Zumi will click on "Sign in with Microsoft" and login using her Office 365 work account.

Two advantages for this model of authentication:

- Jack and Zumi don't need to remember any passcodes for any of the meetings - they just login and once they login next Meet will thanks to Single Sign On allow them to join the Meet without any login.

- Companies where Jack and Zumi are working are in control of their identities and once Jack and Zumi leave the company their login access is revoked etc.

There are 4 oAuth providers supported by Meet in initial release:

- Google,
- Microsoft,
- LinkedIn and
- Facebook

(We will extend the list based on the requests of our partners - any OAuth 2.0 provider can be used)

Finally, participant auth model is not constrained to a single provider but instead can be combination of multiple providers.

Take a look at how Jack Sparrow admin will be able to access his Meet

```pseudo

    {
      user : {
        firstName: "Jack",
        lastName: "Sparrow",
        email: "jack@sparrow.com"
      },
      role: "Admin",
      accessRules: [
        {
          provider: "Google",
          condition: "jack.sparrow@gmail.com"
        },
        {
          provider: "facebook",
          condition: "jacks.sarrow@gmail.com"
        }
      ]
    },

```

He can login with any identity he has with Google, Microsoft, Linkedin, Facebook (as long he used there frank@moody.com email) or simply enter a password.

![alt text](https://meet-cdn.azureedge.net/assets/docs/all_providers.png "Authentication with all providers")

### Guest participants

#### Anonymous guest

There are some use case where we want to have an ability for anyone with a link to join the Meet where he will have some lower level rights compared to other participants.

For example, an educational use case where there is a Brave Startup Founder (BSF) who is doing a webcast every Monday at 3pm where he who would login using his passcode or his oauth and everyone else will just join without any authentication.
He will be the only one who can share screen, who can mute/kick attendees from the Meet etc.

Meet API makes this type of scenarios really simple (note definition of the second participant)

```pseudo

{
  participants: [
    {
      user : {
        firstName: "Brave",
        lastName: "Founder",
        email: "brave@founder.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Microsoft",
          condition: "brave@founder.com"
        }
      ]
    },
    {
      role: "Guest",
    }
  ]
}

```

> NB: Second participant has only role without any access rule definition which is fine as API will create for such entry implicitly.

```pseudo
    accessRole: {
      provider: "Guest"
    }
```

This is how loging screen for this Meet will look like

![alt text](https://meet-cdn.azureedge.net/assets/docs/guest_anon_login.png "Anonymous guest authentication")

#### Password protected anonymous guest

Sometimes Meet needs to be just a bit more constrained so anyone who knows a passcode (eg. shared on Twitter) can access the Meet as guest regardless of their identity  as a "Guest"

```pseudo

{
  participants: [
    {
      user : {
        firstName: "Brave",
        lastName: "Founder",
        email: "brave@founder.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Microsoft",
          condition: "brave@founder.com"
        }
      ]
    },
    {
      role: "Guest",
      accessRules: [
        {
          provider: "Passcode",
          condition: "1234567"
        },
      ]
    }
  ]
}

```

> NB: Only users who know the given passcode can enter the Meet as Guest.

#### Guest with identity

Sometimes you need a Meet where only a specific participant will be allowed to join as a Guest and luckily that too is simple.

```pseudo
{
  participants: [
    {
      user : {
        firstName: "Brave",
        lastName: "Founder",
        email: "brave@founder.com"
      },
      role: "PowerUser",
      accessRules: [
        {
          provider: "Microsoft",
          condition: "brave@founder.com"
        }
      ]
    },
    {
      user : {
        firstName: "John",
        lastName: "Smith",
        email: "john@smith.com"
      },
      role: "Guest",
      accessRules: [
        {
          provider: "Microsoft",
          condition: "john@smith.com"
        }
      ]
    }
  ]
}

```

> NB: John Smith will have to authenticate with Microsoft in order to be allowed allowed to join the Meet but only in a role of a Guest.

## Meet scheduling

### Meeting duration

Every Meet created, by default is created with duration:

- 20 minutes (if a free tier Meet is created)
- 60 minutes (if a normal Meet is created)

If this duration is what you need, you don't need to send any payload in the body of the POST request creating a Meet.

In case you would want a meet with 45 min duration you will need to define scheduling info to contain that non-default duration value

```pseudo
{
    schedulingInfo: {
        "duration": 45
    }
}
```

You can achieve the same when creating thhe Meet in dashboar in General step by changing duration value
![alt text](https://meet-cdn.azureedge.net/assets/docs/pro_general_duration.png "Meet duration definition")

> NB: Creation of the 60 min Meet is free, only the minutes consumed by at least 2 participants are charged.

### Scheduling type

Every Meet can be scheduled in one of the next 3 ways:

- No predefined scheduling
- Manual scheduling
- Automatic schheduling


