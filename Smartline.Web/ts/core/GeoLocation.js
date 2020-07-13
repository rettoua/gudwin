var Core;
(function (Core) {
    var GeoLocation = /** @class */ (function () {
        function GeoLocation() {
        }
        GeoLocation.updateLocation = function (item, index) {
            if (!item) {
                return;
            }
            var url = Ext.String.format("http://geocode-maps.yandex.ru/1.x/?geocode={1},{0}&results=1&format=json", item.b, item.c);
            $.ajax({ url: url }).done(function () {
                try {
                    var street = arguments[0].response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine, divId = '#parking_data' + index, div = $(divId);
                    div.text(div.text() + ' --> ' + street);
                }
                catch (e) { }
            });
        };
        GeoLocation.updateGeoLocationInReport = function (item, index) {
            if (!item) {
                return;
            }
            var url = Ext.String.format("http://geocode-maps.yandex.ru/1.x/?geocode={1},{0}&results=1&format=json", item.b, item.c);
            $.ajax({ url: url }).done(function () {
                try {
                    var street = arguments[0].response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine, divId = '#geo_' + index, div = $(divId);
                    div.text(street);
                }
                catch (e) { }
            });
        };
        return GeoLocation;
    }());
    Core.GeoLocation = GeoLocation;
})(Core || (Core = {}));
