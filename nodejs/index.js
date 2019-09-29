var fetch = require('node-fetch')

const HOST = 'https://int.meet.rs';
const API_KEY = 'YOUR API KEY HERE';
const API_SECRET = 'YOUR API SECRET HERE';

async function createQuickMeet(token) {
    return await fetch(HOST + "/v1/meetings", {
        method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `bearer ${token}`,
            },
            body: JSON.stringify({})
    })
    .then(r => r.json());
}

fetch(HOST + "/v1/token", {
    method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            'grant_type': 'client_credentials',
            'client_key': API_KEY,
            'client_secret': API_SECRET,
        })
})
.then(res => res.json())
.then(ti => createQuickMeet(ti.access_token))
.then(meet => console.log(meet));



    