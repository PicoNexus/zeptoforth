#!/bin/sh

# Copyright (c) 2020 Travis Bemann
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.

VERSION=$1

cp -r . ../zeptoforth-$VERSION
cd ../zeptoforth-$VERSION
mv bin/$VERSION bin-$VERSION
rm -rf bin/*
mv bin-$VERSION bin
rm -rf .git
rm -rf `find . -name 'screenlog.*'`
make clean
rm -rf `find . -name '*~'`
cd ..
tar cfz zeptoforth-$VERSION.tar.gz zeptoforth-$VERSION
