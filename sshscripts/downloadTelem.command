# Ensure directories for the Editor version exist
mkdir ~/Library/Application\ Support/CUED
mkdir ~/Library/Application\ Support/CUED/Leccy
mkdir ~/Library/Application\ Support/CUED/Leccy/AVOWTelemetry

rsync -a dac70@toby.eng.cam.ac.uk:/var/www/leccy/data/AVOW*.* ~/Library/Application\ Support/CUED/Leccy/AVOWTelemetry/downloaded/

# Ensure directories for the standalone player  exist
mkdir ~/Library/Application\ Support/unity.CUED.Leccy
mkdir ~/Library/Application\ Support/unity.CUED.Leccy/AVOWTelemetry

rsync -a dac70@toby.eng.cam.ac.uk:/var/www/leccy/data/AVOW*.* ~/Library/Application\ Support/unity.CUED.Leccy/AVOWTelemetry/downloaded/


