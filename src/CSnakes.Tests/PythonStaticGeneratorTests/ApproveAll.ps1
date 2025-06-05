# Get all .received.txt files in the current directory and subdirectories
$receivedFiles = Get-ChildItem -Path . -Filter *.received.txt -Recurse

foreach ($file in $receivedFiles) {
    # Construct the path for the corresponding .approved.txt file
    $approvedFile = $file.FullName -replace '\.received\.txt$', '.approved.txt'

    # Copy the .received.txt file to .approved.txt, overwriting if it exists
    Copy-Item -Path $file.FullName -Destination $approvedFile -Force
}

Write-Host "All .received.txt files have been copied to .approved.txt."
