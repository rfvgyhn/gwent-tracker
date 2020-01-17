#!/usr/bin/env sh

domain=gwent-tracker
dest=../src/GwentTracker/locale/${domain}.pot

csi XamlToPot.csx ../src/GwentTracker/Views/MainWindow.xaml > ${dest}
