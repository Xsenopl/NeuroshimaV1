<?php
require_once 'db_connect.php';
require_once 'getters.php';

function insertDuel($mysqli, $player1_army, $player2_army) {
    $stmt = $mysqli->prepare("INSERT INTO pojedynki (gracz1_armia, gracz2_armia) VALUES (?, ?)");
    $stmt->bind_param('ss', $player1_army, $player2_army);
    $stmt->execute();

    if ($stmt->affected_rows <= 0) {
        die("2");	//2 = query failed
    }
}

$player1_army = $_POST["player1_army"];
$player2_army = $_POST["player2_army"];

$mysqli = dbConnect();
insertDuel($mysqli, $player1_army, $player2_army);
$id_pojedynku = getLastDuelId($mysqli);

echo "Dodano pojedynek (ID: $id_pojedynku).";

$mysqli->close();
?>