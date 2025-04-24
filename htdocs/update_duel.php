<?php
require_once 'db_connect.php';
require_once 'getters.php';

function updateDuelScore($mysqli, $player1_score, $player2_score) {
    $stmt = $mysqli->prepare("UPDATE pojedynki SET gracz1_wynik = ?, gracz2_wynik = ? WHERE id = (SELECT MAX(id) FROM pojedynki)");
    
	if (!$stmt) {
        die("2");	//2 = query failed
	}
	
	$stmt->bind_param('ii', $player1_score, $player2_score);
    $stmt->execute();
}

$player1_score = isset($_POST['player1_score']) ? (int)$_POST['player1_score'] : 0;
$player2_score = isset($_POST['player2_score']) ? (int)$_POST['player2_score'] : 0;

$mysqli = dbConnect();
updateDuelScore($mysqli, $player1_score, $player2_score);
$id_pojedynku = getLastDuelId($mysqli);

echo "Dodano wynik pojedynku. Gracz1: $player1_score : Gracz2: $player2_score";

$mysqli->close();
?>