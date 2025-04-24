<?php
require_once 'db_connect.php';
require_once 'getters.php';

function readDataBoardJSON() {
    $data = $_POST;
    $boardJson = $data['board'] ?? null;
	$isBefore = isset($data['isBeforeBattle']) && strtolower($data['isBeforeBattle']) === 'true';

    if ($boardJson === null) {
        http_response_code(400);
		die("Brak danych 'board'.");
    }

    $decoded = json_decode($boardJson, true);
    if (!isset($decoded['items']) || !is_array($decoded['items'])) {
        http_response_code(400);
        die("Niepoprawna struktura JSON.");
    }

	// Testy:
    /*echo "<pre>";
    echo "Wartość isBeforeBattle: ";
    var_dump($isBefore);
    echo "\nTokeny:\n";
    print_r($decoded['items']);
    echo "</pre>";
	*/
    return [$decoded['items'], $isBefore];
}

function insertBoard($mysqli, $tableName, $tokens, $id_pojedynku, $nr_bitwy) {
    $stmt = $mysqli->prepare("INSERT INTO `$tableName` (id_pojedynku, nr_bitwy, jednostka, armia, x_pos, y_pos, obrot) VALUES (?, ?, ?, ?, ?, ?, ?)");

    foreach ($tokens as $token) {
        $tokenName = $token['tokenName'] ?? null;
        $army = $token['army'] ?? null;
        $rotation = (int)($token['currentRotation'] ?? 0);
        $x = (int)($token['x'] ?? null);
        $y = (int)($token['y'] ?? null);

        $stmt->bind_param('iissiii', $id_pojedynku, $nr_bitwy, $tokenName, $army, $x, $y, $rotation);
        $stmt->execute();
    }

    $stmt->close();
}

function main() {
	$lockFile = fopen("lockfile.lock", "c");
	
	if (!flock($lockFile, LOCK_EX)) {
        http_response_code(503);
        die("Serwer zajęty flock, spróbuj ponownie później.");
    }
	
	list($tokens, $isBefore) = readDataBoardJSON();
	
	$mysqli = dbConnect();
    $tableName = $isBefore ? "plansza_przed" : "plansza_po";
	$id_pojedynku = getLastDuelId($mysqli);
    $nr_bitwy = getNextBattleNr($mysqli, $id_pojedynku, $tableName);

    insertBoard($mysqli, $tableName, $tokens, $id_pojedynku, $nr_bitwy);

    $mysqli->close();
    echo "Zapisano planszę w tabeli $tableName.";
	flock($lockFile, LOCK_UN);
    fclose($lockFile);
}

main();
?>