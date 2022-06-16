using IniFile.Models.RsTouch;
using IniFileManager.Core;


var path = @"***"; //file ptah + file name.ini
RsTouchRoot rsTouch = new Reader().GetFile<RsTouchRoot>(path);

var path2 = @"***"; //file ptah + file name.ini
new Writer().SetFile<RsTouchRoot>(path2,rsTouch);
