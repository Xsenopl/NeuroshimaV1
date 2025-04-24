<?php
require_once 'db_connect.php';

function registerUser($mysqli, $email, $nick, $password){
	$stmt = $mysqli->prepare("INSERT INTO uzytkownicy (email, nick, haslo) VALUES (?, ?, ?)");
    
	if (!$stmt) {
        //http_response_code(500);
        echo "Błąd prepare():  $mysqli->error";
    }
	
	$stmt->bind_param('sss', $email, $nick, $password);
    $stmt->execute();
	
	if ($stmt->affected_rows <= 0) {
        die("2");	//2 = query failed
    }
	
	$stmt->close();
}


$email = $_POST["email"];
$nick = $_POST["username"];
$password = password_hash($_POST["password"], PASSWORD_DEFAULT);

$mysqli = dbConnect();
registerUser($mysqli, $email, $nick, $password);

echo "Dodano nowego użytkownika $nick.";

$mysqli->close();
?>