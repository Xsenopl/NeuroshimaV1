<?php
// Konfiguracja połączenia z bazą
define('DB_HOST', 'localhost');
define('DB_USER', 'root');
define('DB_PASS', 'root');
define('DB_NAME', 'projekt_inz_neuroshimahex');

function dbConnect() {
    $mysqli = new mysqli(DB_HOST, DB_USER, DB_PASS, DB_NAME);

    if ($mysqli->connect_errno) {
        //die("Błąd połączenia: " . $mysqli->connect_error);
		die("1");	//1 = connection to database failed
    }

    $mysqli->set_charset('utf8mb4');
    return $mysqli;
}
?>