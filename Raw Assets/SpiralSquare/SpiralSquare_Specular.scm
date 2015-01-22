(define (script-fu-spiral-square-spec inImage inLayer inStepPerc inSideLenPass inOverSample)

(let* 
	(
		; define our local variables
		; create a new image
		(inSideLen (* inSideLenPass inOverSample))
		(theImage	(car 
						(gimp-image-new
					 	inSideLen
					 	inSideLen
					 	RGB
					 	)
					)
		)

		; Create the new layer for the image
		(theLayer 
			(car 
				(gimp-layer-new
				 theImage 
				 inSideLen
				 inSideLen
				 RGBA-IMAGE
				 "Layer-1"
				 100
				 NORMAL
				)
			)
		)
		(thePoints (cons-array 4 'double)) ; line_points: Array of 4 doubles
		(theBrushName)
	)
	; End of local variables
	
	;Start the instructions
	(gimp-image-add-layer theImage theLayer 0)
	;(gimp-context-set-background '(128 128 255) )
	;(gimp-drawable-fill theLayer BACKGROUND-FILL)
	
	; draw a square
	(let* 
		(
			(localSideLen (/ inSideLen 2) )
			(radRot 0)
			(sideDec  (/ inStepPerc 100) )
			(op (sqrt (+ (sqr (- sideDec 1) ) 1) ))
		;	(op (sqrt (+ (* 2 (sqr (- sideDec 1) ) ) 2)))
			(radInc (acos (/ (- 2 sideDec) (* (sqrt 2) op))))
		;	(radInc 0.01 )
			(sizeInc (/ op (sqrt 2)))
			(color (list 0 0 0))
		)
		;(gimp-message  (number->string op) )
		;(gimp-message  (number->string (sqrt 2)) )	
		;(gimp-message  (number->string sizeInc) )
		(while (> localSideLen  10)	
			(script-fu-draw-squareNoRivet theLayer (* localSideLen 1.04) (* localSideLen 0.06) (/ inSideLen 2) (/ inSideLen 2) radRot color)
			(script-fu-draw-squareRivetOnly theLayer localSideLen (* localSideLen 0.01) (/ inSideLen 2) (/ inSideLen 2) radRot color inLayer)
			(set! localSideLen (* localSideLen sizeInc ) )
			(set! radRot (- radRot radInc))
		)
	)
	
	; draw a square
	(let* 
		(
			(localSideLen (/ inSideLen 2) )
			(radRot 0)
			(sideDec  (/ inStepPerc 100) )
			(op (sqrt (+ (sqr (- sideDec 1) ) 1) ))
		;	(op (sqrt (+ (* 2 (sqr (- sideDec 1) ) ) 2)))
			(radInc (acos (/ (- 2 sideDec) (* (sqrt 2) op))))
		;	(radInc 0.01 )
			(sizeInc (/ op (sqrt 2)))
			(color (list 255 255 255))
		)
		;(gimp-message  (number->string op) )
		;(gimp-message  (number->string (sqrt 2)) )	
		;(gimp-message  (number->string sizeInc) )
		(while (> localSideLen  10)	
			(script-fu-draw-squareNoRivet theLayer (* localSideLen 0.98) (* localSideLen 0.03) (/ inSideLen 2) (/ inSideLen 2) radRot color)
			(set! localSideLen (* localSideLen sizeInc ) )
			(set! radRot (- radRot radInc))
		)
	)
	;(gimp-message "reach end of loop")
	
	
	(gimp-image-scale theImage inSideLenPass inSideLenPass)

	(gimp-display-new theImage)
	
	
)

)

(define (sqr val)
	(* val val)
)


(define (script-fu-draw-squareNoRivet theLayer sideLen penWidth centreX centreY radRot color)
	(let*
		; Local variables 
		(
			(thePoints (make-vector 10 )) ; line_points: 5 points (back to beginning)
			(theBrushName)		
			(radius (* (- sideLen (/ penWidth 2) ) (sqrt 2)) )
			(p0x)
			(p0y)
		)
		; instructions
		
		; Need to shift things round by 45 degrees
		(set! radRot (+ radRot 0.785398163397448))
		
		; Set up the brush
		(set! theBrushName (car (gimp-brush-new "New_RD_Brush")) )
		;(gimp-brushes-refresh)
		(gimp-brush-set-radius theBrushName penWidth)
		(gimp-brush-set-aspect-ratio theBrushName 0)
		(gimp-brush-set-hardness theBrushName 0.01)
		(gimp-brush-set-shape theBrushName BRUSH-GENERATED-CIRCLE)
		(gimp-context-set-brush theBrushName)
		
		; Temporary variables to help set points
		(set! p0x (* radius (sin radRot) ) )
		(set! p0y (* radius (cos radRot) ) ) 
		
		; Set the points
		(vector-set! thePoints 0  (+ centreX p0x))
		(vector-set! thePoints 1  (+ centreY p0y)) 

		(vector-set! thePoints 2  (+ centreX p0y))
		(vector-set! thePoints 3  (- centreY p0x)) 

		(vector-set! thePoints 4  (- centreX p0x)) 
		(vector-set! thePoints 5  (- centreY p0y)) 
		
		(vector-set! thePoints 6  (- centreX p0y))
		(vector-set! thePoints 7  (+ centreY p0x)) 

		(vector-set! thePoints 8  (+ centreX p0x))
		(vector-set! thePoints 9  (+ centreY p0y)) 
		
		
		(let*;
			; Local vars 
			(
				(sidePoints (make-vector 4 ))
				(sideNum 0)
				(normalX)
				(normalY)
				(normalZ)
				(normalMag)
			)
			
			(while (< sideNum 4)
			 	(vector-set! sidePoints 0 (vector-ref thePoints (* sideNum 2) ) )
			 	(vector-set! sidePoints 1 (vector-ref thePoints (+ (* sideNum 2) 1) ) )
			 	(vector-set! sidePoints 2 (vector-ref thePoints (* (+ sideNum 1) 2)) )
			 	(vector-set! sidePoints 3 (vector-ref thePoints (+ (* (+ sideNum 1) 2) 1) ) )
			 	
	
			 	
				;(gimp-message (string-append "X = " (number->string normalX) "Y = " (number->string normalY) ))
			 	
			 	(draw-line theLayer sidePoints color)

			 	
	
				(set! sideNum (+ sideNum 1) )
			)

		)
		
		

		
		
		; Delete the brush
  	  	(gimp-brush-delete theBrushName)

	)
)


(define (script-fu-draw-squareRivetOnly theLayer sideLen penWidth centreX centreY radRot color inLayer)
	(let*
		; Local variables 
		(
			(thePoints (make-vector 10 )) ; line_points: 5 points (back to beginning)
			(theBrushName)		
			(radius (* (- sideLen (/ penWidth 2) ) (sqrt 2)) )
			(p0x)
			(p0y)
		)
		; instructions
		
		; Need to shift things round by 45 degrees
		(set! radRot (+ radRot 0.785398163397448))
		
		; Set up the brush
		(set! theBrushName (car (gimp-brush-new "New_RD_Brush")) )
		;(gimp-brushes-refresh)
		(gimp-brush-set-radius theBrushName penWidth)
		(gimp-brush-set-aspect-ratio theBrushName 0)
		(gimp-brush-set-hardness theBrushName 0.01)
		(gimp-brush-set-shape theBrushName BRUSH-GENERATED-CIRCLE)
		(gimp-context-set-brush theBrushName)
		
		; Temporary variables to help set points
		(set! p0x (* radius (sin radRot) ) )
		(set! p0y (* radius (cos radRot) ) ) 
		
		; Set the points
		(vector-set! thePoints 0  (+ centreX p0x))
		(vector-set! thePoints 1  (+ centreY p0y)) 

		(vector-set! thePoints 2  (+ centreX p0y))
		(vector-set! thePoints 3  (- centreY p0x)) 

		(vector-set! thePoints 4  (- centreX p0x)) 
		(vector-set! thePoints 5  (- centreY p0y)) 
		
		(vector-set! thePoints 6  (- centreX p0y))
		(vector-set! thePoints 7  (+ centreY p0x)) 

		(vector-set! thePoints 8  (+ centreX p0x))
		(vector-set! thePoints 9  (+ centreY p0y)) 
		
		
		(let*;
			; Local vars 
			(
				(sidePoints (make-vector 4 ))
				(sideNum 0)
				(normalX)
				(normalY)
				(normalZ)
				(normalMag)
			)
			
			(while (< sideNum 4)
			 	(vector-set! sidePoints 0 (vector-ref thePoints (* sideNum 2) ) )
			 	(vector-set! sidePoints 1 (vector-ref thePoints (+ (* sideNum 2) 1) ) )
			 	(vector-set! sidePoints 2 (vector-ref thePoints (* (+ sideNum 1) 2)) )
			 	(vector-set! sidePoints 3 (vector-ref thePoints (+ (* (+ sideNum 1) 2) 1) ) )
			 	
	
			 	
				;(gimp-message (string-append "X = " (number->string normalX) "Y = " (number->string normalY) ))
			 	
			 	
			 	(let* (
			 		(x0  (vector-ref sidePoints 0))
			 		(y0  (vector-ref sidePoints 1))
			 		(rivetCentreX (+ x0 (* 0.15 (- centreX x0 ) )))
			 		(rivetCentreY (+ y0 (* 0.15 (- centreY y0 ) )))
			 		(xMin (- rivetCentreX (* sideLen 0.05) ))
			 		(yMin (- rivetCentreY (* sideLen 0.05) ))
			 		(xMax (+ rivetCentreX (* sideLen 0.05) ))
			 		(yMax (+ rivetCentreY (* sideLen 0.05) ))
			 		
			 		)
			 	
			 		(script-fu-paste-rivet2 inLayer theLayer xMin yMin xMax yMax)
			 	)					
				(set! sideNum (+ sideNum 1) )
			)

		)
		
		

		
		
		; Delete the brush
  	  	(gimp-brush-delete theBrushName)

	)
)
(define (script-fu-paste-rivet2 inLayer outLayer xMin yMin xMax yMax)
	(let* 
	    (
	    	(test 0)
			(selection)
		)
		(gimp-edit-copy inLayer)
		(set! selection (car (gimp-edit-paste outLayer TRUE)))
		(gimp-item-transform-scale selection xMin yMin xMax yMax)
		(gimp-floating-sel-anchor selection)
	)
)



(define (draw-line inLayer points color)


	 ; set the color
	 (gimp-context-set-foreground color )
			 	
	; Draw the line
	(gimp-paintbrush inLayer 0 4 points PAINT-CONSTANT 0)

)

(script-fu-register
	"script-fu-spiral-square-spec"						; name of the function
	"Spiral Square Specular"						; Menu label
	"Creates in image with a spiralling square"		; Description
	"Diarmid Campbell"								; author
	"Copyright 2015 Diarmid Campbell. CUED." 		; copyright notice
	"January 20, 2015"								; date created
	""												; Image type that the script works on
	SF-IMAGE		"Image"		0				; a string variable
	SF-DRAWABLE		"Layer"		0				; a font variable
	SF-ADJUSTMENT	"Rotate percentage" '(40 1 100 1 10 0 1) 	; a spin button
	SF-ADJUSTMENT	"Side Length" '(1024 1 1920 1 10 0 1) 	; a spin button
	SF-ADJUSTMENT	"Oversample" '(4 1 16 1 1 0 1) 	; a spin button
)
(script-fu-menu-register "script-fu-spiral-square-spec" "<Image>/File/Create/Text")