<?php
function getLastDuelId($mysqli) {
    $result = $mysqli->query("SELECT MAX(id) AS max_id FROM pojedynki");
    $row = $result->fetch_assoc();
    return $row['max_id'];
}
function getLastBattleNr($mysqli, $id_pojedynku) {
    $stmt = $mysqli->prepare("SELECT MAX(nr_bitwy) AS max_nr_bitwy FROM plansza_przed WHERE id_pojedynku = ?");
    $stmt->bind_param('i', $id_pojedynku);
    $stmt->execute();
    $result = $stmt->get_result();
    $row = $result->fetch_assoc();

    return $row['max_nr_bitwy'];
}

function getNextBattleNr($mysqli, $id_pojedynku, $tableName) {
	//Anty SQL Injection
    $allowTables = ['plansza_przed', 'plansza_po'];
    if (!in_array($tableName, $allowTables)) {
        die("2"); //2 = query failed 
    }
	
	$query = "SELECT MAX(nr_bitwy) AS max_nr_bitwy FROM `$tableName` WHERE id_pojedynku = ?";
    $stmt = $mysqli->prepare($query);
    $stmt->bind_param('i', $id_pojedynku);
    $stmt->execute();
    $result = $stmt->get_result();
    $row = $result->fetch_assoc();

    return ($row['max_nr_bitwy'] !== null) ? $row['max_nr_bitwy'] + 1 : 1;
}


?>