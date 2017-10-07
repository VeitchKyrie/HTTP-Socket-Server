function sendValues(verb, content, rootverb)
{
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function ()
    {
        if (this.status == 200 || this.status == 204 || this.status == 404)
        {
            document.getElementById("success").innerHTML = this.responseText;
        }
    };

    if (birthdate.checkValidity() && birthdate.value != "" || verb == 'DELETE')
    {
        xhttp.open(verb, "/api/user", true);
        xhttp.setRequestHeader("Content-type", "text/xml");

        if (content != ' ')
        {
            xhttp.send(content);
            username.value = "";
            realname.value = "";
            birthdate.value = "";
        }
        else
            xhttp.send();

        success.textContent = "User has been " + rootverb + "ed successfully"
    }
    else
    {
        birthdate.value = birthdate.defaultValue;
        success.textContent = "Error on " + rootverb + "ing the user!"
    }
}

function getUser()
{
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function ()
    {
        if (this.status == 200) {
            document.getElementById("users").innerHTML = this.responseText;
        }
    };

    xhttp.open("GET", "/api/users", true);
    xhttp.setRequestHeader("Content-type", "text/xml");
    xhttp.send();
}

function deleteUser()
{
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function ()
    {
        if (this.status == 200 || this.status == 404)
        {
            document.getElementById("success").innerHTML = this.responseText;
        }
    };

    xhttp.open("DELETE", "/api/user", true);
    xhttp.setRequestHeader("Content-type", "text/xml");
    xhttp.send();
}

function goTo(url)
{
    window.open(url, "_self");
}