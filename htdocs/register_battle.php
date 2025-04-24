<?php
require_once 'db_connect.php';
require_once 'getters.php';

function insertBattleEvent($mysqli, $id_pojedynku, $nr_bitwy){
	echo $nr_bitwy;
	$battlePhase = $_POST['battlePhase'] ?? null;
    $attackerX = $_POST['attackerX'] ?? null;
    $attackerY = $_POST['attackerY'] ?? null;
    $targetX = $_POST['targetX'] ?? null;
    $targetY = $_POST['targetY'] ?? null;

    //if ($battlePhase === null || $attackerX === null || $attackerY === null || $targetX === null || $targetY === null) {
    //    http_response_code(400);
    //    die("Brakuje danych bitwy (battlePhase, attackerX/Y, targetX/Y)");
    //}

    $stmt = $mysqli->prepare("INSERT INTO bitwy (id_pojedynku, nr_bitwy, nr_inicjatywy, atacker_x_pos, atacker_y_pos, target_x_pos, target_y_pos) VALUES (?, ?, ?, ?, ?, ?, ?)");
    $stmt->bind_param("iiiiiii", $id_pojedynku, $nr_bitwy, $battlePhase, $attackerX, $attackerY, $targetX, $targetY);
    $stmt->execute();
    $stmt->close();
}

function getBattleAttacksFromPost() {
    $json = $_POST['battle'] ?? null;

    if ($json === null) {
        http_response_code(400);
        die("Brak danych 'battle'.");
    }

    $decoded = json_decode($json, true);

    if (!isset($decoded['items']) || !is_array($decoded['items'])) {
        http_response_code(400);
        die("Niepoprawna struktura JSON – oczekiwano tablicy 'items'.");
    }

    return $decoded['items'];
}

function insertBattleSequence($mysqli, $id_pojedynku, $nr_bitwy, $attacks) {
    $stmt = $mysqli->prepare("INSERT INTO bitwy (id_pojedynku, nr_bitwy, nr_inicjatywy, atacker_x_pos, atacker_y_pos, target_x_pos, target_y_pos) VALUES (?, ?, ?, ?, ?, ?, ?)");

    foreach ($attacks as $attack) {
        $nr_inicjatywy = $attack['battlePhase'] ?? null;
        $atacker_x_pos = $attack['attackerX'] ?? null;
        $atacker_y_pos = $attack['attackerY'] ?? null;
        $target_x_pos = $attack['targetX'] ?? null;
        $target_y_pos = $attack['targetY'] ?? null;

        if ($nr_inicjatywy === null || $atacker_x_pos === null || $atacker_y_pos === null || $target_x_pos === null || $target_y_pos === null) {
            continue; // pomija niepełny wpis
        }

        $stmt->bind_param("iiiiiii", $id_pojedynku, $nr_bitwy, $nr_inicjatywy, $atacker_x_pos, $atacker_y_pos, $target_x_pos, $target_y_pos);
        $stmt->execute();
    }

    $stmt->close();
}

function main(){
	$lockFile = fopen("lockfile.lock", "c");
	
	if (!flock($lockFile, LOCK_EX)) {
        http_response_code(503);
        die("Serwer zajęty flock, spróbuj ponownie później.");
    }
	
	$mysqli = dbConnect();
	$id_pojedynku = getLastDuelId($mysqli);
    $nr_bitwy = getLastBattleNr($mysqli, $id_pojedynku);
	
	if (isset($_POST['battle'])) {
        $attacks = getBattleAttacksFromPost();
        insertBattleSequence($mysqli, $id_pojedynku, $nr_bitwy, $attacks);
        echo "Zapisano sekwencję ataków. nr bitwy: $nr_bitwy.";
    }
	
	$mysqli->close();
	flock($lockFile, LOCK_UN);
    fclose($lockFile);
}

main();
?>