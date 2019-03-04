import cv2
import os
import numpy as np
from PIL import Image

import matplotlib.mlab as mlab
import matplotlib.pyplot as plt

import pickle
import csv

if __name__ == "__main__":

    outputPath = "F:\\data\\sat_imgs_10000_41.2380599975586_-88.6040496826172_40.8813323974609_-88.1886291503906"
    minpixelX , minpixelY , maxpixelX , maxpixelY = 34074880,50203904,34229504,50380032
    
    #img = np.zeros(((maxpixelY-minpixelY+256)/2,(maxpixelX-minpixelX+256)/2,3),dtype=np.uint8)
    
    filename = ""
    with open('F:\\data\\sat_imgs_41.2380599975586_-88.6040496826172_40.8813323974609_-88.1886291503906.txt', 'rb') as csvfile:
        spamreader = csv.reader(csvfile, delimiter=',', quotechar='|')
        for row in spamreader:
            quadKey ,tileX ,tileY ,level, pixelX ,pixelY ,lat ,lon = row
            cimg = cv2.imread("F:\\data\\sat_imgs_41.2380599975586_-88.6040496826172_40.8813323974609_-88.1886291503906\\a"+quadKey+".jpeg")
            x = int(pixelY)-minpixelY
            y = int(pixelX)-minpixelX

            idx = int(x / 12800)
            idy = int(y / 12800)

            tfile = os.path.join(outputPath,"%d_%d.jpg" % (idx,idy))
            if tfile != filename:
                if (filename !=""):
                    cv2.imwrite(filename,timg)     
                if os.path.isfile(tfile):
                    timg = cv2.imread(tfile)
                else:
                    timg = np.zeros((12800,12800,3),dtype=np.uint8)
                filename = tfile
            offsetx = x-idx*12800;
            offsety = y-idy*12800;
            timg[offsetx:offsetx+256,offsety:offsety+256,:] = cimg
               

    