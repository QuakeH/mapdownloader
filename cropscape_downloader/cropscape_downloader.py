from pyproj import Proj, transform
import urllib
import re

inProj = Proj(init='epsg:4326')
outProj = Proj(init='epsg:5070')



lon1,lat1 = -88.604050,41.238060
lon2,lat2 = -88.188629,40.881333

lon1,lat1 = -117.489852905273,47.328119405234
lon2,lat2 = -116.983795166016,46.8831923266462

lon1,lat1 = -120.299606323242,36.5245359150048
lon2,lat2 = -119.09797668457,35.922420347285


lon1,lat1 = -91.1844635009766,34.9619345057769
lon2,lat2 = -90.6063079833984,34.4363631809337


#bbox: n,s,e,w

nlon1,nlat1 = transform(inProj,outProj,lon1,lat1)
nlon2,nlat2 = transform(inProj,outProj,lon2,lat2)


#https://nassgeodata.gmu.edu/CropScape/GetImage?dataurl=https://nassgeodata.gmu.edu/nass_data_cache/CDL_2018_clip_20190221193158_2110788401.zip&latlonbbox=-87.843488,41.078876,-87.717759,41.14771&projection=EPSG:102004
print("%d,%d" % (nlon1,nlat1))
print("%d,%d" % (nlon1,nlat2))
print("%d,%d" % (nlon2,nlat2))
print("%d,%d" % (nlon2,nlat1))
print("%d,%d" % (nlon1,nlat1))

bbstring = "%d,%d,%d,%d" %(nlon1,nlat2,nlon2,nlat1)

print(bbstring)

year = 2018

import ssl

# This restores the same behavior as before.

def get_cdl_image():
    for year in range(2012,2019):
        cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLFile?year=%d&bbox=%s" % (year,bbstring)
        print(cdlurl)
        context = ssl._create_unverified_context()
        content = urllib.urlopen(cdlurl, context=context).read()
        imgurl = re.sub(".*<returnURL>","",content)
        imgurl = re.sub("</returnURL>.*","",imgurl)
        urllib.urlretrieve(imgurl, "F:\data\cdl_%d_%f_%f_%f_%f.tif" % (year,lon1,lat1,lon2,lat2), context=context)

def get_cdl_stat():
    for year in range(2012,2019):
        cdlurl = "https://nassgeodata.gmu.edu/axis2/services/CDLService/GetCDLStat?year=%d&bbox=%s&format=json" % (year,bbstring)
        print(cdlurl)
        context = ssl._create_unverified_context()
        content = urllib.urlopen(cdlurl, context=context).read()
        imgurl = re.sub(".*<returnURL>","",content)
        imgurl = re.sub("</returnURL>.*","",imgurl)
        urllib.urlretrieve(imgurl, "F:\data\cdl_stat_%d_%f_%f_%f_%f.json" % (year,lon1,lat1,lon2,lat2), context=context)

get_cdl_image()
get_cdl_stat()